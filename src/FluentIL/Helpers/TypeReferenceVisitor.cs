//using Mono.Cecil;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace FluentIL.Helpers
//{
//    public abstract class TypeReferenceVisitor
//    {
//        public virtual TypeReference Visit(TypeReference reference)
//        {
//            switch (reference)
//            {
//                case GenericParameter type: reference = VisitGenericArgument(type); break;
//                case ByReferenceType type: reference = VisitByReferenceType(type); break;
//                case GenericInstanceType type: reference = VisitGenericInstanceType(type); break;
//                case SentinelType type: reference = VisitSentinelType(type); break;
//                case ArrayType type: reference = VisitArrayType(type); break;
//                case FunctionPointerType type: reference = VisitFunctionPointerType(type); break;
//                case OptionalModifierType type: reference = VisitOptionalModifierType(type); break;
//                case RequiredModifierType type: reference = VisitRequiredModifierType(type); break;
//                case PointerType type: reference = VisitPointerType(type); break;
//                case PinnedType type: reference = VisitPinnedType(type); break;
//                default: reference = VisitTypeReference(reference); break;
//            }

//            for (int i = 0; i < reference.GenericParameters.Count; i++)
//                reference.GenericParameters[i] = VisitGenericParameter(reference.GenericParameters[i]);


//            return reference;
//        }

//        private TypeReference VisitGenericArgument(GenericParameter type)
//        {
//            throw new NotImplementedException();
//        }

//        private TypeReference VisitPinnedType(PinnedType type)
//        {
//            throw new NotImplementedException();
//        }

//        private TypeReference VisitPointerType(PointerType type)
//        {
//            throw new NotImplementedException();
//        }


//        private TypeReference VisitRequiredModifierType(RequiredModifierType type)
//        {
//            throw new NotImplementedException();
//        }

//        private TypeReference VisitOptionalModifierType(OptionalModifierType optionalModifierType)
//        {
//            throw new NotImplementedException();
//        }

//        private TypeReference VisitFunctionPointerType(FunctionPointerType functionPointerType)
//        {
//            throw new NotImplementedException();
//        }

//        internal abstract TypeReference VisitArrayType(ArrayType arrayType);

//        private TypeReference VisitSentinelType(SentinelType sentinel)
//        {
//            throw new NotImplementedException();
//        }

//        protected virtual TypeReference VisitGenericInstanceType(GenericInstanceType type)
//        {
//            var modified = false;
//            var element = Visit(type.ElementType);
//            if (element != type.ElementType) modified = true;

//            var arguments = type.GenericArguments.Select(a =>
//            {
//                var na = Visit(a);
//                if (a != na) modified = true;
//                return na;
//            }).ToArray();

//            if (modified)
//            {
//                type = new GenericInstanceType(element);
//                foreach (var arg in arguments) type.GenericArguments.Add(arg);
//            }

//            return type;
//        }
//        protected virtual TypeReference VisitByReferenceType(ByReferenceType type)
//        {
//            var element = Visit(type.ElementType);
//            return element == type.ElementType ? type : new ByReferenceType(element);
//        }
//        protected virtual GenericParameter VisitGenericParameter(GenericParameter gp)
//        {
//            foreach (var gc in gp.Constraints)
//                ngp.Constraints.Add(gc.Clone(ngp));


//            var ngp = new GenericParameter(gparam.Name, target)
//            {
//                Attributes = gparam.Attributes,
//                //Namespace = gparam.Namespace,
//                IsValueType = gparam.IsValueType,
//                //MetadataToken = gparam.MetadataToken
//            };

            

//            return ngp;
//        }

//        protected virtual TypeReference VisitTypeReference(TypeReference reference)
//        {
//            var declaringType = reference.DeclaringType;
//            if (declaringType != null)
//                declaringType = Visit(declaringType);


//        }
//    }
//}
