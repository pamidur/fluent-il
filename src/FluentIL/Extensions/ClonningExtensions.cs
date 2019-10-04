using Mono.Cecil;
using System;

namespace FluentIL.Extensions
{
    public static class ClonningExtensions
    {
        public static MethodDefinition Clone(this MethodDefinition origin, TypeDefinition target)
        {
            var method = new MethodDefinition(origin.Name,
               origin.Attributes & ~MethodAttributes.RTSpecialName,
               origin.ReturnType);

            foreach (var gparam in origin.GenericParameters)
                method.GenericParameters.Add(gparam.Clone(method));

            if (origin.ReturnType.IsGenericParameter && ((GenericParameter)origin.ReturnType).Owner == origin)
                method.ReturnType = method.GenericParameters[origin.GenericParameters.IndexOf((GenericParameter)origin.ReturnType)];

            if (origin.IsSpecialName)
                method.IsSpecialName = true;

            foreach (var parameter in origin.Parameters)
            {
                var paramType = parameter.ParameterType;
                if (paramType.IsGenericParameter && ((GenericParameter)paramType).Owner == origin)
                    paramType = method.GenericParameters[origin.GenericParameters.IndexOf((GenericParameter)paramType)];

                method.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, paramType));
            }

            return method;
        }

        public static T Clone<T>(this T reference, IGenericParameterProvider genericProvider)
            where T : TypeReference
        {
            TypeReference newtr = null;

            if (reference is GenericParameter gp) newtr = gp.CloneGenericParam(genericProvider);
            else if (reference is ByReferenceType byref) newtr = new ByReferenceType(byref.ElementType.Clone(genericProvider));
            else if (reference is GenericInstanceType) throw new Exception();
            else if (reference is SentinelType st) newtr = new SentinelType(st.Clone(genericProvider));
            else if (reference is ArrayType at) newtr = new ArrayType(at.ElementType.Clone(genericProvider));
            else if (reference is FunctionPointerType) throw new Exception();
            else if (reference is OptionalModifierType) throw new Exception();
            else if (reference is RequiredModifierType) throw new Exception();
            else if (reference is PointerType) throw new Exception();
            else if (reference is PinnedType) throw new Exception();
            else if (reference is PointerType) throw new Exception();
            else newtr = new TypeReference(reference.Namespace, reference.Name, genericProvider.Module, reference.Scope, reference.IsValueType);

            foreach (var subgp in reference.GenericParameters)
                newtr.GenericParameters.Add(subgp.Clone(newtr));

            return (T)newtr;
        }

        private static GenericParameter CloneGenericParam(this GenericParameter gparam, IGenericParameterProvider target)
        {
            var ngp = new GenericParameter(gparam.Name, target)
            {
                HasDefaultConstructorConstraint = gparam.HasDefaultConstructorConstraint,
                HasReferenceTypeConstraint = gparam.HasReferenceTypeConstraint,
                HasNotNullableValueTypeConstraint = gparam.HasNotNullableValueTypeConstraint,
                IsNonVariant = gparam.IsNonVariant,
                IsContravariant = gparam.IsContravariant,
                IsCovariant = gparam.IsCovariant,
                //Namespace = gparam.Namespace,
                IsValueType = gparam.IsValueType,
                //MetadataToken = gparam.MetadataToken
            };

            foreach (var gc in gparam.Constraints)
                ngp.Constraints.Add(gc.Clone(ngp));

            return ngp;
        }
    }
}
