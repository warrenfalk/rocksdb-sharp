/*
 License: MIT - see license file at https://github.com/warrenfalk/auto-native-import/blob/master/LICENSE
 Author: Warren Falk <warren@warrenfalk.com>
 */
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace NativeImport
{
    public static class Auto
    {
        /// <summary>
        /// Imports the library by name (without extensions) locating it based on platform.
        /// 
        /// Use <code>suppressUnload</code> to prevent the dll from unloading at finalization,
        /// which can be useful if you need to call the imported functions in finalizers of
        /// other instances and can't predict in which order the finalization will occur
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="suppressUnload">true to prevent unloading on finalization</param>
        /// <returns></returns>
        public static T Import<T>(string name, string version, bool suppressUnload = false) where T : class
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Importers.Import<T>(Importers.Windows, name, version, suppressUnload);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Importers.Import<T>(Importers.Posix, name, version, suppressUnload);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Importers.Import<T>(Importers.Posix, name, version, suppressUnload);
            else
                return Importers.Import<T>(Importers.Windows, name, version, suppressUnload);
        }
    }

    public interface INativeLibImporter
    {
        IntPtr LoadLibrary(string name);
        IntPtr GetProcAddress(IntPtr lib, string entryPoint);
        void FreeLibrary(IntPtr lib);
        string Translate(string name);
    }

    public static class Importers
    {
        public static INativeLibImporter Windows = new WindowsImporter();
        public static INativeLibImporter Posix = new PosixImporter();

        private class WindowsImporter : INativeLibImporter
        {
            [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
            public static extern IntPtr WinLoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
            public static extern IntPtr WinGetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
            public static extern bool WinFreeLibrary(IntPtr hModule);

            public IntPtr LoadLibrary(string path)
            {
                IntPtr lib = WinLoadLibrary(path);
                if (lib == IntPtr.Zero)
                    throw new NativeLoadException("LoadLibrary: unable to load library at " + path, null);
                return lib;
            }

            public IntPtr GetProcAddress(IntPtr lib, string entryPoint)
            {
                var address = WinGetProcAddress(lib, entryPoint);
                return address;
            }

            public void FreeLibrary(IntPtr lib)
            {
                WinFreeLibrary(lib);
            }

            public string Translate(string name)
            {
                return name + ".dll";
            }
        }

        private class PosixImporter : INativeLibImporter
        {
            public string LibraryExtension { get; }

            [DllImport("libdl")]
            private static extern IntPtr dlopen(String fileName, int flags);

            [DllImport("libdl")]
            private static extern IntPtr dlsym(IntPtr handle, String symbol);

            [DllImport("libdl")]
            private static extern int dlclose(IntPtr handle);

            [DllImport("libdl")]
            private static extern IntPtr dlerror();

            [DllImport("libc")]
            private static extern int uname(IntPtr buf);

            public PosixImporter()
            {
                var platform = GetPlatform();
                if (platform.StartsWith("Darwin"))
                    LibraryExtension = "dylib";
                else
                    LibraryExtension = "so";
            }

            static string GetPlatform()
            {
                IntPtr buf = IntPtr.Zero;
                try
                {
                    buf = Marshal.AllocHGlobal(8192);
                    return (0 == uname(buf)) ? Marshal.PtrToStringAnsi(buf) : "Unknown";
                }
                catch
                {
                    return "Unknown";
                }
                finally
                {
                    if (buf != IntPtr.Zero)
                        Marshal.FreeHGlobal(buf);
                }
            }
            public IntPtr LoadLibrary(string path)
            {
                dlerror();
                IntPtr lib = dlopen(path, 2);
                var errPtr = dlerror();
                if (errPtr != IntPtr.Zero)
                    throw new NativeLoadException("dlopen: " + Marshal.PtrToStringAnsi(errPtr), null);
                return lib;
            }

            public IntPtr GetProcAddress(IntPtr lib, string entryPoint)
            {
                dlerror();
                IntPtr address = dlsym(lib, entryPoint);
                var errPtr = dlerror();
                if (errPtr != IntPtr.Zero)
                    throw new NativeLoadException("dlsym: " + Marshal.PtrToStringAnsi(errPtr), null);
                return address;
            }

            public void FreeLibrary(IntPtr lib)
            {
                dlclose(lib);
            }

            public string Translate(string name)
            {
                return "lib" + name + "." + LibraryExtension;
            }
        }

        public static class U
        {
            public static Delegate LoadFunc(INativeLibImporter importer, IntPtr libraryHandle, string entryPoint, Type delegateType)
            {
                IntPtr procAddress = importer.GetProcAddress(libraryHandle, entryPoint);
                if (procAddress == IntPtr.Zero)
                    throw new NativeLoadException(string.Format("Unable to get address of {0} ({1})", entryPoint, delegateType), null);
                return Marshal.GetDelegateForFunctionPointer(procAddress, delegateType);
            }
        }

        public static string GetArchName(Architecture arch)
        {
            switch (arch)
            {
                case Architecture.X86:
                    return "i386";
                case Architecture.X64:
                    return "amd64";
                default:
                    return arch.ToString().ToLower();
            }
        }

        public static T Import<T>(INativeLibImporter importer, string libName, string version, bool suppressUnload) where T : class
        {
            var subdir = GetArchName(RuntimeInformation.ProcessArchitecture);

            var assemblyName = new AssemblyName("DynamicLink");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, System.Reflection.Emit.AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynLinkModule");
            string typeName = typeof(T).Name + "_impl";
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, typeof(T));

            FieldBuilder field_importer = typeBuilder.DefineField("importer", typeof(INativeLibImporter), FieldAttributes.Private | FieldAttributes.InitOnly);
            FieldBuilder field_libraryHandle = typeBuilder.DefineField("libraryHandle", typeof(IntPtr), FieldAttributes.Private | FieldAttributes.InitOnly);

            var methods = typeof(T).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.IsAbstract && !m.IsGenericMethod).ToArray();

            // Define delegate types for each of the method signatures
            var delegateMap = new Dictionary<string, Type>();
            foreach (var method in methods)
            {
                var sig = GetMethodSig(method);
                if (delegateMap.ContainsKey(sig))
                    continue;
                var delegateTypeInfo = CreateDelegateType(moduleBuilder, method);
                delegateMap.Add(sig, delegateTypeInfo.AsType());
            }

            // Define one field for each method to hold a delegate
            var delegates = methods.Select(m => new {
                MethodInfo = m,
                DelegateType = delegateMap[GetMethodSig(m)],
            }).ToArray();
            var fields = delegates.Select(d => typeBuilder.DefineField(d.MethodInfo.Name + "_func", d.DelegateType, FieldAttributes.Private)).ToArray();


            // Create the constructor which will initialize the importer and library handle
            // and also use the importer to populate each of the delegate fields
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[] { typeof(INativeLibImporter), typeof(string) });

            {
                var baseConstructor = typeof(T).GetTypeInfo().GetConstructor(new Type[0]);
                ILGenerator il = constructor.GetILGenerator();

                // Call base constructor
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Call, baseConstructor);

                // Store importer field
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldarg_1); // importer
                il.Emit(OpCodes.Stfld, field_importer);

                // Load and store library handle
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldarg_1); // importer
                il.Emit(OpCodes.Ldarg_2); // name
                il.Emit(OpCodes.Callvirt, typeof(INativeLibImporter).GetTypeInfo().GetMethod("LoadLibrary"));
                il.Emit(OpCodes.Stfld, field_libraryHandle);

                // Initialize each delegate field
                for (int i = 0; i < fields.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldarg_1); // importer
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldfld, field_libraryHandle);
                    il.Emit(OpCodes.Ldstr, delegates[i].MethodInfo.Name); // use method name from original class as entry point
                    il.Emit(OpCodes.Ldtoken, delegates[i].DelegateType); // the delegate type
                    il.Emit(OpCodes.Call, typeof(System.Type).GetTypeInfo().GetMethod("GetTypeFromHandle")); // typeof()
                    il.Emit(OpCodes.Call, typeof(U).GetTypeInfo().GetMethod("LoadFunc")); // U.LoadFunc()
                    il.Emit(OpCodes.Isinst, delegates[i].DelegateType); // as <delegate type>
                    il.Emit(OpCodes.Stfld, fields[i]);
                }

                // End of constructor
                il.Emit(OpCodes.Ret);
            }

            // Create destructor
            var destructor = typeBuilder.DefineMethod("Finalize", MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig);
            {
                var baseDestructor = typeof(T).GetTypeInfo().GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance);
                var il = destructor.GetILGenerator();
                var end = il.DefineLabel();

                il.BeginExceptionBlock();
                if (!suppressUnload)
                {
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldfld, field_importer); // .importer
                    il.Emit(OpCodes.Ldarg_0); // this
                    il.Emit(OpCodes.Ldfld, field_libraryHandle); // .libraryHandle
                    il.Emit(OpCodes.Callvirt, typeof(INativeLibImporter).GetTypeInfo().GetMethod("FreeLibrary")); // INativeLibImporter::FreeLibrary()
                }
                //il.Emit(OpCodes.Leave, end);
                il.BeginFinallyBlock();
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Call, baseDestructor); // object::Finalize()
                //il.Emit(OpCodes.Endfinally);
                il.EndExceptionBlock();
                il.MarkLabel(end);
                il.Emit(OpCodes.Ret);
            }

            // Now override each method from the base class
            for (int i = 0; i < fields.Length; i++)
            {
                var baseMethod = delegates[i].MethodInfo;
                var args = baseMethod.GetParameters();
                var omethod = typeBuilder.DefineMethod(
                    baseMethod.Name, 
                    (baseMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.NewSlot)) | MethodAttributes.Virtual, 
                    baseMethod.CallingConvention, 
                    baseMethod.ReturnType, 
                    args.Select(arg => arg.ParameterType).ToArray()
                );
                var il = omethod.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldfld, fields[i]); // {field}
                if (args.Length >= 1)
                    il.Emit(OpCodes.Ldarg_1);
                if (args.Length >= 2)
                    il.Emit(OpCodes.Ldarg_2);
                if (args.Length >= 3)
                    il.Emit(OpCodes.Ldarg_3);
                for (short argNum = 4; argNum <= args.Length; argNum++)
                    il.Emit(OpCodes.Ldarg_S, argNum);
                il.Emit(OpCodes.Tailcall);
                il.Emit(OpCodes.Callvirt, delegates[i].DelegateType.GetTypeInfo().GetMethod("Invoke"));
                il.Emit(OpCodes.Ret);
            }

            var type = typeBuilder.CreateTypeInfo();

            var versionParts = version.Split('.');
            var names = versionParts.Select((p, i) => libName + "-" + string.Join(".", versionParts.Take(i + 1)))
                .Reverse()
                .Concat(Enumerable.Repeat(libName, 1));

            // try to load locally
            var paths = new[]
            {
                Path.Combine("native", subdir),
                "native",
                subdir,
                "",
            };

            var basePath = Path.GetDirectoryName(typeof(PosixImporter).GetTypeInfo().Assembly.Location);
            var search = paths.SelectMany(path => names.Select(n => Path.Combine(basePath, path, importer.Translate(n))))
                .Concat(names.Select(n => importer.Translate(n)))
                .ToArray();

            foreach (var spec in search)
            {
                try
                {
                    var construct = type.GetConstructor(new Type[] { typeof(INativeLibImporter), typeof(string) });
                    var obj = construct.Invoke(new object[] { importer, spec });
                    var t = obj as T;
                    return t;
                }
                catch (Exception)
                {
                }
            }

            throw new NativeLoadException("Unable to locate rocksdb native library, either install it, or use RocksDbNative nuget package\nSearched:" + string.Join("\n", search), null);
        }

        private static string GetMethodSig(MethodInfo m)
        {
            return string.Join("_", Enumerable.Repeat(m.ReturnType.Name, 1).Concat(m.GetParameters().Select(p => p.ParameterType.Name)));
        }

        private static TypeInfo CreateDelegateType(ModuleBuilder moduleBuilder, MethodInfo methodTemplate)
        {
            var sig = GetMethodSig(methodTemplate);
            var typeBuilder = moduleBuilder.DefineType(sig, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed, typeof(System.MulticastDelegate));
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[] { typeof(Object), typeof(IntPtr) });
            constructor.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var parameters = methodTemplate.GetParameters().Select(pi => pi.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, methodTemplate.ReturnType, parameters);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            return typeBuilder.CreateTypeInfo();
        }

    }

    public class NativeLoadException : Exception
    {
        public NativeLoadException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

