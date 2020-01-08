using Mono.Cecil;
using System;

namespace FluentIL.Extensions
{
    public static class ClonningExtensions
    {
        public static T Clone<T>(this T reference, IGenericParameterProvider genericProvider)
            where T : TypeReference
        {
            TypeReference newtr = null;

            if (reference is GenericParameter gp) newtr = gp.CloneGenericParam(genericProvider);
            else if (reference is ByReferenceType byref) newtr = new ByReferenceType(byref.ElementType.Clone(genericProvider));
            else if (reference is GenericInstanceType git)
            {
                var newgitr = new GenericInstanceType(git.ElementType.Clone(genericProvider));
                foreach (var ga in git.GenericArguments)
                    newgitr.GenericArguments.Add(ga.Clone(newgitr));
                newtr = newgitr;
            }
            else if (reference is SentinelType st) newtr = new SentinelType(st.Clone(genericProvider));
            else if (reference is ArrayType at) newtr = new ArrayType(at.ElementType.Clone(genericProvider));
            else if (reference is FunctionPointerType) throw new Exception();
            else if (reference is OptionalModifierType) throw new Exception();
            else if (reference is RequiredModifierType) throw new Exception();
            else if (reference is PointerType) throw new Exception();
            else if (reference is PinnedType) throw new Exception();
            else if (reference is PointerType) throw new Exception();
            else newtr = new TypeReference(reference.Namespace, reference.Name, genericProvider.Module, reference.Scope, reference.IsValueType)
            {
                DeclaringType = reference.DeclaringType?.Clone(genericProvider)
            };

            foreach (var subgp in reference.GenericParameters)
                newtr.GenericParameters.Add(subgp.Clone(newtr));

            return (T)newtr;
        }

        private static GenericParameter CloneGenericParam(this GenericParameter gparam, IGenericParameterProvider target)
        {
            var ngp = new GenericParameter(gparam.Name, target)
            {
                Attributes = gparam.Attributes,
                //Namespace = gparam.Namespace,
                IsValueType = gparam.IsValueType,
                //MetadataToken = gparam.MetadataToken
            };

            foreach (var gc in gparam.Constraints)
                ngp.Constraints.Add(new GenericParameterConstraint(gc.ConstraintType.Clone(ngp)));

            return ngp;
        }
    }
}
