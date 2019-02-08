using System;
using System.Reflection;
using System.Reflection.Emit;

namespace XamlX.TypeSystem
{
    public static class TypeSystemExtensions
    {
        public static IXamlXEmitter Ldarg(this IXamlXEmitter emitter, int arg)
            => emitter.Emit(OpCodes.Ldarg, arg);

        public static IXamlXEmitter Ldarg_0(this IXamlXEmitter emitter)
            => emitter.Emit(OpCodes.Ldarg_0);
        
        public static IXamlXEmitter Ldfld(this IXamlXEmitter emitter, IXamlXField field)
            => emitter.Emit(OpCodes.Ldfld, field);
        
        public static IXamlXEmitter LdThisFld(this IXamlXEmitter emitter, IXamlXField field)
            => emitter.Ldarg_0().Emit(OpCodes.Ldfld, field);
        
        public static IXamlXEmitter Stfld(this IXamlXEmitter emitter, IXamlXField field)
            => emitter.Emit(OpCodes.Stfld, field);

        public static IXamlXEmitter Ldloc(this IXamlXEmitter emitter, IXamlXLocal local)
            => emitter.Emit(OpCodes.Ldloc, local);
        
        public static IXamlXEmitter Stloc(this IXamlXEmitter emitter, IXamlXLocal local)
            => emitter.Emit(OpCodes.Stloc, local);

        public static IXamlXEmitter Ldnull(this IXamlXEmitter emitter) => emitter.Emit(OpCodes.Ldnull);

        public static IXamlXEmitter Ldstr(this IXamlXEmitter emitter, string arg)
            => emitter.Emit(OpCodes.Ldstr, arg);
        
        public static IXamlXEmitter Ldc_I4(this IXamlXEmitter emitter, int arg)
            => arg == 0
                ? emitter.Emit(OpCodes.Ldc_I4_0)
                : arg == 1
                    ? emitter.Emit(OpCodes.Ldc_I4_1)
                    : emitter.Emit(OpCodes.Ldc_I4, arg);

        public static IXamlXEmitter Beq(this IXamlXEmitter emitter, IXamlXLabel label)
            => emitter.Emit(OpCodes.Beq, label);
        
        public static IXamlXEmitter Blt(this IXamlXEmitter emitter, IXamlXLabel label)
            => emitter.Emit(OpCodes.Blt, label);
        
        public static IXamlXEmitter Ble(this IXamlXEmitter emitter, IXamlXLabel label)
            => emitter.Emit(OpCodes.Ble, label);
        
        public static IXamlXEmitter Bgt(this IXamlXEmitter emitter, IXamlXLabel label)
            => emitter.Emit(OpCodes.Bgt, label);
        
        public static IXamlXEmitter Bge(this IXamlXEmitter emitter, IXamlXLabel label)
            => emitter.Emit(OpCodes.Bge, label);
        
        public static IXamlXEmitter Br(this IXamlXEmitter emitter, IXamlXLabel label)
            => emitter.Emit(OpCodes.Br, label);
        
        public static IXamlXEmitter Brfalse(this IXamlXEmitter emitter, IXamlXLabel label)
            => emitter.Emit(OpCodes.Brfalse, label);
        
        public static IXamlXEmitter Brtrue(this IXamlXEmitter emitter, IXamlXLabel label)
            => emitter.Emit(OpCodes.Brtrue, label);
        
        public static IXamlXEmitter Ret(this IXamlXEmitter emitter)
            => emitter.Emit(OpCodes.Ret);
        
        public static IXamlXEmitter Dup(this IXamlXEmitter emitter)
            => emitter.Emit(OpCodes.Dup);
        
        public static IXamlXEmitter Ldtoken(this IXamlXEmitter emitter, IXamlXType type)
            => emitter.Emit(OpCodes.Ldtoken, type);
        
        public static IXamlXEmitter Ldtoken(this IXamlXEmitter emitter, IXamlXMethod method)
            => emitter.Emit(OpCodes.Ldtoken, method);

        public static IXamlXEmitter Ldtype(this IXamlXEmitter emitter, IXamlXType type)
        {
            var conv = emitter.TypeSystem.GetType("System.Type")
                .FindMethod(m => m.IsStatic && m.IsPublic && m.Name == "GetTypeFromHandle");
            return emitter.Ldtoken(type).EmitCall(conv);
        }
        
        public static IXamlXEmitter LdMethodInfo(this IXamlXEmitter emitter, IXamlXMethod method)
        {
            var conv = emitter.TypeSystem.GetType("System.Reflection.MethodInfo")
                .FindMethod(m => m.IsStatic && m.IsPublic && m.Name == "GetMethodFromHandle");
            return emitter.Ldtoken(method).EmitCall(conv);
        }





    }
}