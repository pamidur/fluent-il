//using Mono.Cecil;
//using Mono.Cecil.Rocks;
//using System;
//using System.Linq;

//namespace FluentIL.Extensions
//{
//    public static class GenericProcessingExtension
//    {

//        public static TypeReference ResolveIfGeneric(this MemberReference member, TypeReference param)
//        {
//            if (param.ContainsGenericParameter)
//            {
//                var gparam = member.ResolveGenericType(param);
//                return gparam;
//            }

//            return param;
//        }

//        private static TypeReference ResolveGenericType(this MemberReference member, TypeReference param)
//        {
//            if (!param.ContainsGenericParameter)
//                throw new Exception($"{param} is not generic!");

//            if (param.IsByReference && param.ContainsGenericParameter)
//                return new ByReferenceType(member.ResolveGenericType(param.GetElementType()));

//            if (param.IsGenericInstance)
//            {
//                var nestedGeneric = (GenericInstanceType)param;
//                var args = nestedGeneric.GenericArguments.Select(ga => member.ResolveIfGeneric(ga)).ToArray();
//                return param.Module.ImportReference(param.Resolve()).MakeGenericInstanceType(args);
//            }

//            if (!(param is GenericParameter gparam))
//                throw new Exception("Cannot resolve generic parameter");

//            object resolvedMember = ((dynamic)member).Resolve();
//            object resolvedOwner = ((dynamic)gparam.Owner).Resolve();

//            if (resolvedOwner == resolvedMember)
//            {
//                if (member is IGenericInstance)
//                    return (member as IGenericInstance).GenericArguments[gparam.Position];
//                else
//                    return ((IGenericParameterProvider)member).GenericParameters[gparam.Position];
//            }
//            else if (member.DeclaringType != null)
//                return member.DeclaringType.ResolveGenericType(gparam);
//            //else 
//            else
//                throw new Exception("Cannot resolve generic parameter");
//        }
//    }
//}