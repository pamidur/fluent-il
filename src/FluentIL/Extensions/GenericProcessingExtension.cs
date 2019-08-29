using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace FluentIL.Extensions
{
    public static class GenericProcessingExtension
    {
        public static MethodReference MakeGenericReference(
                                  this MethodDefinition self,
                                  TypeReference context)
        {
            var reference = new MethodReference(
                self.Name,
                self.ReturnType,
                context.MakeCallReference(self.DeclaringType))
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };
            foreach (var parameter in self.Parameters)
            {
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            }

            foreach (var genericParam in self.GenericParameters)
            {
                reference.GenericParameters.Add(genericParam.Clone(self));
            }

            return reference;
        }

        public static FieldReference MakeCallReference(this MemberReference member, FieldReference reference)
        {
            var result = 
             member.Module.ImportReference(new FieldReference(reference.Name, member.Module.ImportReference(reference.FieldType), member.MakeCallReference(reference.DeclaringType)));
            return result;
        }

        public static MethodReference MakeCallReference(this MemberReference member, MethodReference reference)
        {
            if (reference.GenericParameters.Count == 0)
                return member.Module.ImportReference(reference);

            var result = new GenericInstanceMethod(reference);
            var args = reference.GenericParameters.Select(gp => member.GetMatchedParam(gp) ?? gp).ToList();
            args.ForEach(result.GenericArguments.Add);
            return member.Module.ImportReference(result);
        }

        public static TypeReference MakeCallReference(this MemberReference member, TypeReference reference)
        {
            if (reference.GenericParameters.Count == 0)
                return member.Module.ImportReference(reference);

            var args = reference.GenericParameters.Select(gp => member.GetMatchedParam(gp) ?? gp).ToArray();

            return member.ParametrizeGenericInstance(reference.Resolve().MakeGenericInstanceType(args));
        }

        public static GenericParameter GetMatchedParam(this MemberReference provider, GenericParameter original)
        {
            if (provider is MethodReference)
            {
                var match = ((MethodReference)provider).GenericParameters.FirstOrDefault(gp => gp.Name == original.Name);
                match = match ?? provider.DeclaringType.GetMatchedParam(original);
                return match;
            }
            else if (provider is TypeReference)
            {
                return ((TypeReference)provider).GenericParameters.FirstOrDefault(gp => gp.Name == original.Name);
            }

            throw new Exception($"Non supported generic provider {provider.GetType()}");
        }

        public static GenericInstanceType ParametrizeGenericInstance(this MemberReference member, GenericInstanceType generic)
        {
            if (!generic.ContainsGenericParameter)
                return generic;

            var args = generic.GenericArguments.Select(ga => member.ResolveIfGeneric(ga)).ToArray();

            return generic.Resolve().MakeGenericInstanceType(args);
        }

        public static TypeReference ResolveIfGeneric(this MemberReference member, TypeReference param)
        {
            if (param.ContainsGenericParameter)
            {
                var gparam = member.ResolveGenericType(param);
                return gparam;
            }

            return param;
        }

        public static TypeReference ResolveGenericType(this MemberReference member, TypeReference param)
        {
            if (!param.ContainsGenericParameter)
                throw new Exception($"{param} is not generic!");

            if (param.IsByReference && param.ContainsGenericParameter)
                return new ByReferenceType(member.ResolveGenericType(param.GetElementType()));

            if (param.IsGenericInstance)
            {
                var nestedGeneric = (GenericInstanceType)param;
                var args = nestedGeneric.GenericArguments.Select(ga => member.ResolveIfGeneric(ga)).ToArray();
                return param.Module.ImportReference(param.Resolve()).MakeGenericInstanceType(args);
            }

            if (!(param is GenericParameter gparam))
                throw new Exception("Cannot resolve generic parameter");

            object resolvedMember = ((dynamic)member).Resolve();
            object resolvedOwner = ((dynamic)gparam.Owner).Resolve();

            if (resolvedOwner == resolvedMember)
            {
                if (member is IGenericInstance)
                    return (member as IGenericInstance).GenericArguments[gparam.Position];
                else
                    return ((IGenericParameterProvider)member).GenericParameters[gparam.Position];
            }
            else if (member.DeclaringType != null)
                return member.DeclaringType.ResolveGenericType(gparam);
            //else 
            else
                throw new Exception("Cannot resolve generic parameter");
        }
    }
}