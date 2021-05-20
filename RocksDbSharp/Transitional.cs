using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// The purpose of this file is to ease the transition from framework to framework.
/// As much as possible, in this shared code project, we'll try to use .Net Core compatible code
/// And then add code here to make that work.
/// When not possible, we'll create our own wrapper functions and then create different implementations
/// based on preprocessor defines
/// </summary>

#if NET40
namespace System
{
    /// <summary>
    /// Shim in the new TypeInfo API as just a reference back to Type
    /// </summary>
    internal class TypeInfo
    {
        private Type Type { get; }

        internal TypeInfo(Type type)
        {
            Type = type;
        }

        internal MethodInfo GetMethod(string name) => Type.GetMethod(name);

        internal MethodInfo GetMethod(string name, BindingFlags flags) => Type.GetMethod(name, flags);

        internal MethodInfo[] GetMethods(BindingFlags bindingFlags) => Type.GetMethods(bindingFlags);

        public Type AsType() => Type;

        public ConstructorInfo[] GetConstructors() => Type.GetConstructors();

        public ConstructorInfo GetConstructor(Type[] args) => Type.GetConstructor(args);

        public Assembly Assembly => Type.Assembly;
    }
}
#endif

namespace Transitional
{
    internal static class CurrentFramework
    {
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
#if NET40
            => AppDomain.CurrentDomain.DefineDynamicAssembly(name, access);
#else
            => AssemblyBuilder.DefineDynamicAssembly(name, access);
#endif

        public static unsafe string CreateString(sbyte* value, int startIndex, int length, System.Text.Encoding enc)
#if NETSTANDARD1_6
        {           
            int vlength = enc.GetCharCount((byte*)value, length);
            if (vlength == 0)
            {
                return string.Empty;
            }
            fixed (char* v = new char[vlength])
            {
                enc.GetChars((byte*)value, length, v, vlength);
                return new string(v, 0, vlength);
            }
        }
#else
            => new string(value, startIndex, length, enc);
#endif

        public static T GetDelegateForFunctionPointer<T>(IntPtr ptr)
#if NETSTANDARD1_6
            => Marshal.GetDelegateForFunctionPointer<T>(ptr);
#else
            => (T)(object)Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
#endif

        public static IntPtr GetFunctionPointerForDelegate<T>(T func)
#if NETSTANDARD1_6
            => Marshal.GetFunctionPointerForDelegate<T>(func);
#else
            => Marshal.GetFunctionPointerForDelegate((Delegate)(object)func);
#endif

        public static string GetBaseDirectory()
#if NETSTANDARD1_6
            => AppContext.BaseDirectory;
#else
            => AppDomain.CurrentDomain.BaseDirectory;
#endif

    }

    internal static class TransitionalExtensions
    {
#if NET40 || NETSTANDARD1_6
        public static long GetLongLength<T>(this T[] array, int dimension) => array.GetLength(dimension);
#endif

        public static T CreateDelegate<T>(this MethodInfo methodInfo)
#if NET40
            => (T)(object)Delegate.CreateDelegate(typeof(T), methodInfo);
#else
            => (T)(object)methodInfo.CreateDelegate(typeof(T));
#endif


#if NET40
        public static TypeInfo CreateTypeInfo(this TypeBuilder builder)
        {
            var type = builder.CreateType();
            return new TypeInfo(type);
        }

        public static TypeInfo GetTypeInfo(this Type type)
        {
            return new TypeInfo(type);
        }
#endif

#if NET45 || NET40
        public unsafe static string GetString(this Encoding encoding, byte* bytes, int count)
        {
            var ptr = new IntPtr((void*)bytes);
            if (ReferenceEquals(encoding, Encoding.ASCII))
                return Marshal.PtrToStringAnsi(ptr, count);
            // An ugly double copy which is necessary in older frameworks without the unsafe GetString()
            byte[] temp = new byte[count];
            Marshal.Copy(ptr, temp, 0, count);
            return encoding.GetString(temp);
        }
#endif
    }
}


//#if !NETSTANDARD1_6
//namespace System.Runtime.InteropServices
//{
//    public static class OSPlatform
//    {
//        public static string Linux { get; } = "Linux";
//        public static string OSX { get; } = "OSX";
//        public static string Windows { get; } = "Windows";
//    }
//
//    public enum Architecture
//    {
//        X86 = 0,
//        X64 = 1,
//        Arm = 2,
//        Arm64 = 3
//    }
//
//    internal static class RuntimeInformation
//    {
//        internal static bool IsOSPlatform(string osplatform)
//        {
//            switch ((int)Environment.OSVersion.Platform)
//            {
//                case (int)PlatformID.Win32Windows: // Win9x supported?
//                case (int)PlatformID.Win32S: // Win16 NTVDM on Win x86?
//                case (int)PlatformID.Win32NT: // Windows NT
//                case (int)PlatformID.WinCE:
//                    return osplatform == OSPlatform.Windows;
//                case (int)PlatformID.Unix:
//                    return osplatform == OSPlatform.Linux;
//                case (int)PlatformID.MacOSX:
//                case 128: // Mono Mac
//                    return osplatform == OSPlatform.OSX;
//                default:
//                    return false;
//            }
//        }
//
//        internal static Architecture ProcessArchitecture => Environment.Is64BitProcess? Architecture.X64 : Architecture.X86;
//    }
//}
//#endif