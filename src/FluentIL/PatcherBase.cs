using FluentIL.Common;
using FluentIL.Logging;
using FluentIL.Resolvers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace FluentIL
{
    public abstract class PatcherBase
    {
        protected readonly ILogger _log;

        public PatcherBase(
            ILogger logger)
        {
            _log = logger;
        }

        public void Process(string assemblyFile, IReadOnlyList<string> references, bool optimize)
        {
            var resolver = GetResolver(assemblyFile, references);
            if (_log.IsErrorThrown) return;

            Process(assemblyFile, resolver, optimize);
        }

        public void Process(string assemblyFile, IAssemblyResolver resolver, bool optimize)
        {
            if (!File.Exists(assemblyFile))
            {
                _log.Log(GenericErrorRule, $"Target not found: '{assemblyFile}'");
                return;
            }
            
            _log.Log(GenericInfoRule, $"Started for {Path.GetFileName(assemblyFile)}");

            var pdbPresent = AreSymbolsFound(assemblyFile);
            var assembly = ReadAssembly(assemblyFile, resolver, pdbPresent);

            var modified = PatchAssembly(assembly, optimize);

            if (!_log.IsErrorThrown)
            {
                if (modified)
                {
                    foreach (var m in assembly.Modules)
                        StandardTypes.UpdateCoreLibRef(m);

                    _log.Log(GenericInfoRule, "Assembly has been patched.");
                    WriteAssembly(assembly, assemblyFile, pdbPresent);
                }
                else _log.Log(GenericInfoRule, "No patching required.");
            }
        }

        protected virtual IAssemblyResolver GetResolver(string assemblyFile, IReadOnlyList<string> references)
        {
            var resolver = new KnownReferencesAssemblyResolver();

            if (!File.Exists(assemblyFile)) _log.Log(GenericErrorRule, $"Target not found: '{assemblyFile}'");
            else resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyFile));

            foreach (var refr in references)
            {
                if (!File.Exists(refr)) _log.Log(GenericErrorRule, $"Reference not found: '{refr}'");
                else resolver.AddReference(refr);
            }

            return resolver;
        }

        private AssemblyDefinition ReadAssembly(string assemblyFile, IAssemblyResolver resolver, bool readSymbols)
        {
            var assembly = AssemblyDefinition.ReadAssembly(assemblyFile,
                new ReaderParameters
                {
                    ReadingMode = ReadingMode.Deferred
                });
            var name = assembly.Name;
            assembly.Dispose();

            assembly = resolver.Resolve(assembly.Name, new ReaderParameters
            {
                ReadingMode = ReadingMode.Deferred,
                ReadWrite = true,
                AssemblyResolver = resolver,
                ReadSymbols = readSymbols
            });

            _log.Log(GenericInfoRule, "Assembly has been read.");

            return assembly;
        }

        private void WriteAssembly(AssemblyDefinition assembly, string path, bool writeSymbols)
        {
            var param = new WriterParameters();

            if (writeSymbols)
            {
                param.WriteSymbols = true;

                //if (assembly.MainModule.SymbolReader != null)
                //    param.SymbolWriterProvider = assembly.MainModule.SymbolReader.GetWriterProvider();
            }

            assembly.Write(param);

            if (assembly.MainModule.SymbolReader != null)
                assembly.MainModule.SymbolReader.Dispose();

            assembly.Dispose();
            assembly = null;

            _log.Log(GenericInfoRule, "Assembly has been written.");
        }

        private bool AreSymbolsFound(string dllPath)
        {
            var pdbPath = Path.Combine(Path.GetDirectoryName(dllPath), Path.GetFileNameWithoutExtension(dllPath) + ".pdb");

            if (File.Exists(pdbPath))
            {
                return true;
            }

            _log.Log(GenericInfoRule, $"Symbols not found on {pdbPath}. Proceeding without...");
            return false;
        }

        protected abstract Rule GenericInfoRule { get; }
        protected abstract Rule GenericErrorRule { get; }

        protected abstract bool PatchAssembly(AssemblyDefinition assembly, bool optimize);
    }
}