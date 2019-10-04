using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace FluentIL.Extensions
{
    public static class MemberDefinitionExtensions
    {
        public static bool IsCallCompatible(this MemberReference member)
        {
            if (
                (member is MethodReference method && method.HasGenericParameters) ||
                (member.DeclaringType != null && member.DeclaringType.HasGenericParameters)
                )
                return false;

            return true;
        }

        public static TypeReference MakeReference(this TypeDefinition definition)
        {
            if (definition.HasGenericParameters)
                return definition.MakeGenericInstanceType(definition.GenericParameters.ToArray());
            return definition;
        }

        public static FieldReference MakeReference(this FieldDefinition definition, TypeReference type)
        {
            type = type is TypeDefinition td ? td.MakeReference() : type;
            return new FieldReference(definition.Name, definition.FieldType, type);
        }

        public static MethodReference MakeReference(this MethodDefinition definition, TypeReference type)
        {
            type = type is TypeDefinition td ? td.MakeReference() : type;

            var reference = new MethodReference(definition.Name, definition.ReturnType, type);
         
            foreach (var par in definition.Parameters)
                reference.Parameters.Add(par);
            foreach (var gpar in definition.GenericParameters)
                reference.GenericParameters.Add(gpar);
            //if (definition.HasGenericParameters)
            //    reference = reference.MakeGenericInstanceMethod(definition.GenericParameters.ToArray());

            return reference;
        }

        public static GenericInstanceMethod MakeGenericInstanceMethod(this MethodReference self, params TypeReference[] arguments)
        {
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
