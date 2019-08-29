using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace FluentIL.Extensions
{
    public static class MemberDefinitionExtensions
    {
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
    }
}
