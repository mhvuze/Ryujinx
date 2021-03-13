using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ARMeilleure.Translation
{
    static class MethodHelpers
    {
        private const string DelegateTypesAssemblyName = "JitDelegateTypes";

        private static ModuleBuilder _modBuilder;

        private static ConcurrentDictionary<(string, object), Delegate> _delegatesCache;
        private static ConcurrentDictionary<string, Type> _delegateTypesCache;

        static MethodHelpers()
        {
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(DelegateTypesAssemblyName), AssemblyBuilderAccess.Run);

            _modBuilder = asmBuilder.DefineDynamicModule(DelegateTypesAssemblyName);

            _delegatesCache = new ConcurrentDictionary<(string, object), Delegate>();
            _delegateTypesCache = new ConcurrentDictionary<string, Type>();
        }

        public static IntPtr GetFunctionPointerForNativeCode(MethodInfo meth, object instance = null)
        {
            string funcName = GetFullName(meth);

            Delegate dlg = _delegatesCache.GetOrAdd((funcName, instance), (_) =>
            {
                Type[] parameters = meth.GetParameters().Select(x => x.ParameterType).ToArray();

                Type delegateType = GetDelegateType(parameters, meth.ReturnType);

                return Delegate.CreateDelegate(delegateType, instance, meth);
            });

            return Marshal.GetFunctionPointerForDelegate<Delegate>(dlg);
        }

        private static string GetFullName(MethodInfo meth)
        {
            return $"{meth.DeclaringType.FullName}.{meth.Name}";
        }

        private static Type GetDelegateType(Type[] parameters, Type returnType)
        {
            string key = GetFunctionSignatureKey(parameters, returnType);

            return _delegateTypesCache.GetOrAdd(key, (_) => MakeDelegateType(parameters, returnType, key));
        }

        private const MethodAttributes CtorAttributes =
            MethodAttributes.RTSpecialName |
            MethodAttributes.HideBySig |
            MethodAttributes.Public;

        private const MethodImplAttributes ImplAttributes =
            MethodImplAttributes.Runtime |
            MethodImplAttributes.Managed;

        private const MethodAttributes InvokeAttributes =
            MethodAttributes.Public |
            MethodAttributes.HideBySig |
            MethodAttributes.NewSlot |
            MethodAttributes.Virtual;

        private const TypeAttributes DelegateTypeAttributes =
            TypeAttributes.Class |
            TypeAttributes.Public |
            TypeAttributes.Sealed |
            TypeAttributes.AnsiClass |
            TypeAttributes.AutoClass;

        private static readonly Type[] _delegateCtorSignature = { typeof(object), typeof(IntPtr) };

        private static Type MakeDelegateType(Type[] parameters, Type returnType, string name)
        {
            TypeBuilder builder = _modBuilder.DefineType(name, DelegateTypeAttributes, typeof(MulticastDelegate));

            builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _delegateCtorSignature).SetImplementationFlags(ImplAttributes);

            builder.DefineMethod("Invoke", InvokeAttributes, returnType, parameters).SetImplementationFlags(ImplAttributes);

            return builder.CreateTypeInfo();
        }

        private static string GetFunctionSignatureKey(Type[] parameters, Type returnType)
        {
            string sig = GetTypeName(returnType);

            foreach (Type type in parameters)
            {
                sig += '_' + GetTypeName(type);
            }

            return sig;
        }

        private static string GetTypeName(Type type)
        {
            return type.FullName.Replace(".", string.Empty);
        }
    }
}