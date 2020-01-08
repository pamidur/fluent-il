using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;

namespace FluentIL.Extensions
{
    public static class MemberDefinitionExtensions
    {
        public static bool IsCallCompatible(this MemberReference member)
        {
            if (member is TypeReference typeRef)
                return !typeRef.HasGenericParameters;

            if (member is MethodReference methodRef)
                return !methodRef.HasGenericParameters && 
                methodRef.DeclaringType.IsCallCompatible();

            if (member is FieldReference fieldRef)
                return fieldRef.DeclaringType.IsCallCompatible();

            throw new Exception();
        }        

        public static TypeReference MakeSelfReference(this TypeDefinition definition)
        {
            TypeReference reference = definition;
            if (definition.HasGenericParameters)
                reference = definition.MakeGenericInstanceType(definition.GenericParameters.ToArray());
            return reference;
        }

        public static FieldReference MakeReference(this FieldDefinition definition, TypeReference ownerTypeRef)
        {
            if (!ownerTypeRef.IsCallCompatible())
                throw new Exception();

            return new FieldReference(definition.Name, definition.FieldType, ownerTypeRef);
        }

        public static MethodReference MakeReference(this MethodReference definition, TypeReference ownerTypeRef)
        {
            if (!ownerTypeRef.IsCallCompatible())
                throw new Exception();


            var reference = new MethodReference(definition.Name, definition.ReturnType, ownerTypeRef);


            reference.HasThis = definition.HasThis;
            reference.ExplicitThis = definition.ExplicitThis;
            reference.CallingConvention = definition.CallingConvention;

            foreach (var gpar in definition.GenericParameters)
                reference.GenericParameters.Add(gpar.Clone(reference));

            foreach (var par in definition.Parameters)
                reference.Parameters.Add(par);


            //reference = new DefaultMetadataImporter(ownerTypeRef.Module).ImportReference(definition, reference);
            //reference.DeclaringType = ownerTypeRef;

            //if (definition.HasGenericParameters)
            //    reference = reference.MakeGenericInstanceMethod(definition.GenericParameters.ToArray());

            return reference;
        }

        public static GenericInstanceMethod MakeGenericInstanceMethod(this MethodReference self, params TypeReference[] arguments)
        {
            if (self.IsDefinition)
                throw new Exception();

            if (self == null)
            {
                throw new ArgumentNullException("self");
            }
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            if (arguments.Length == 0)
            {
                throw new ArgumentException();
            }
            if (self.GenericParameters.Count != arguments.Length)
            {
                throw new ArgumentException();
            }
            var genericInstanceMethod = new GenericInstanceMethod(self);
            foreach (TypeReference item in arguments)
            {
                genericInstanceMethod.GenericArguments.Add(item);
            }

            return genericInstanceMethod;
        }
    }
}
