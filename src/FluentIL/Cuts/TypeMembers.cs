using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace FluentIL
{
    public static class TypeMembers
    {
        public static Cut ThisOrStatic(this Cut cut) =>
            cut.Method.HasThis ? cut.This() : cut;

        public static Cut ThisOrNull(this Cut cut) =>
            cut.Method.HasThis ? cut.This() : cut.Null();

        public static Cut This(this Cut cut)
        {
            if (cut.Method.HasThis) return cut.Write(OpCodes.Ldarg_0);
            else throw new Exception("Attempt to load 'this' on static method.");
        }

        public static Cut Load(this Cut cut, VariableDefinition variable) => cut
            .Write(OpCodes.Ldloc, variable);

        public static Cut LoadRef(this Cut cut, VariableDefinition variable) => cut
            .Write(OpCodes.Ldloca, variable);

        public static Cut Store(this Cut cut, VariableDefinition variable, PointCut value = null) => cut
            .Here(value)
            .Write(OpCodes.Stloc, variable);


        public static Cut Load(this Cut cut, FieldReference field)
        {
            CheckReference(field);
            var fieldDef = field.Resolve();

            return cut.Write(fieldDef.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
        }

        public static Cut LoadRef(this Cut cut, FieldReference field)
        {
            CheckReference(field);
            var fieldDef = field.Resolve();

            return cut.Write(fieldDef.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, field);
        }

        public static Cut Store(this Cut cut, FieldReference field, PointCut value = null)
        {
            CheckReference(field);
            var fieldDef = field.Resolve();

            return cut
                .Here(value)
                .Write(fieldDef.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
        }

        public static Cut Load(this Cut cut, ParameterReference par)
        {
            return cut.Write(OpCodes.Ldarg, par.Resolve());
        }

        public static Cut LoadRef(this Cut cut, ParameterReference par)
        {
            return cut.Write(OpCodes.Ldarga, par.Resolve());
        }

        public static Cut Store(this Cut cut, ParameterReference par, PointCut value = null)
        {
            if (par.ParameterType.IsByReference)
            {
                return cut
                    .Load(par)
                    .Here(value);
            }
            else
            {
                return cut
                    .Here(value)
                    .Write(OpCodes.Starg, par.Resolve());
            }
        }

        private static void CheckReference(MemberReference member)
        {
            if (
                (member is MethodReference method && method.HasGenericParameters) ||
                (member.DeclaringType != null && member.DeclaringType.HasGenericParameters)
                )
                throw new ArgumentException($"Uninitialized generic call reference: {member.ToString()}");
        }
    }
}
