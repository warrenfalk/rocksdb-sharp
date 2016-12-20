using System;
using System.Reflection;
using System.Reflection.Emit;

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
    }

    internal static class TransitionalExtensions
    {
#if NET40 || NETCORE1
        public static long GetLongLength<T>(this T[] array, int dimension) => array.GetLength(dimension);
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
    }
}


#if !NETCORE
namespace System.Runtime.InteropServices
{
    public static class OSPlatform
    {
        public static string Linux { get; } = "Linux";
        public static string OSX { get; } = "OSX";
        public static string Windows { get; } = "Windows";
    }

    public enum Architecture
    {
        X86 = 0,
        X64 = 1,
        Arm = 2,
        Arm64 = 3
    }

    internal static class RuntimeInformation
    {
        internal static bool IsOSPlatform(string osplatform)
        {
            switch ((int)Environment.OSVersion.Platform)
            {
                case (int)PlatformID.Win32Windows: // Win9x supported?
                case (int)PlatformID.Win32S: // Win16 NTVDM on Win x86?
                case (int)PlatformID.Win32NT: // Windows NT
                case (int)PlatformID.WinCE:
                    return osplatform == OSPlatform.Windows;
                case (int)PlatformID.Unix:
                    return osplatform == OSPlatform.Linux;
                case (int)PlatformID.MacOSX:
                case 128: // Mono Mac
                    return osplatform == OSPlatform.OSX;
                default:
                    return false;
            }
        }

        internal static Architecture ProcessArchitecture => Environment.Is64BitProcess? Architecture.X64 : Architecture.X86;
    }
}
#endif