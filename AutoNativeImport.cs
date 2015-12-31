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
        public static T Import<T>(string name) where T : class
        {
            var subdir = Environment.Is64BitProcess ? "amd64" : "i386";
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "native", subdir, name);
            switch ((int)Environment.OSVersion.Platform)
            {
                case (int)PlatformID.Win32Windows: // Win9x supported?
                case (int)PlatformID.Win32S: // Win16 NTVDM on Win x86?
                case (int)PlatformID.Win32NT: // Windows NT
                case (int)PlatformID.WinCE:
                    return Importers.Load<T>(Importers.Windows, path + ".dll");
                case (int)PlatformID.MacOSX:
                case 128: // Mono Mac
                    return Importers.Load<T>(Importers.Posix, path + ".dylib");
                case (int)PlatformID.Unix:
                    return Importers.Load<T>(Importers.Posix, path + ".so");
                default:
                    return Importers.Load<T>(Importers.Windows, path);
            }

        }
    }

    public interface INativeLibImporter
    {
        IntPtr LoadLibrary(string name);
        IntPtr GetProcAddress(IntPtr lib, string entryPoint);
        void FreeLibrary(IntPtr lib);
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
        }

        private class PosixImporter : INativeLibImporter
        {
            [DllImport("libdl.so")]
            private static extern IntPtr dlopen(String fileName, int flags);

            [DllImport("libdl.so")]
            private static extern IntPtr dlsym(IntPtr handle, String symbol);

            [DllImport("libdl.so")]
            private static extern int dlclose(IntPtr handle);

            [DllImport("libdl.so")]
            private static extern IntPtr dlerror();

            public IntPtr LoadLibrary(string path)
            {
                dlerror();
                IntPtr lib = dlopen(path, 2);
                var errPtr = dlerror();
                if (errPtr != IntPtr.Zero)
                    throw new Exception("dlopen: " + Marshal.PtrToStringAnsi(errPtr));
                return lib;
            }

            public IntPtr GetProcAddress(IntPtr lib, string entryPoint)
            {
                dlerror();
                IntPtr address = dlsym(lib, entryPoint);
                var errPtr = dlerror();
                if (errPtr != IntPtr.Zero)
                    throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
                return address;
            }

            public void FreeLibrary(IntPtr lib)
            {
                dlclose(lib);
            }
        }

        public static class U
        {
            public static Delegate LoadFunc(INativeLibImporter importer, IntPtr libraryHandle, string entryPoint, Type delegateType)
            {
                return Marshal.GetDelegateForFunctionPointer(importer.GetProcAddress(libraryHandle, entryPoint), delegateType);
            }
        }

        // TODO: refactor: rename to Import
        public static T Load<T>(INativeLibImporter importer, string name) where T : class
        {
            var assemblyName = new AssemblyName("DynamicLink");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynLinkModule", "dynamic.dll");
            string typeName = typeof(T).Name + "_impl";
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, typeof(T));

            FieldBuilder field_importer = typeBuilder.DefineField("importer", typeof(INativeLibImporter), FieldAttributes.Private | FieldAttributes.InitOnly);
            FieldBuilder field_libraryHandle = typeBuilder.DefineField("libraryHandle", typeof(IntPtr), FieldAttributes.Private | FieldAttributes.InitOnly);

            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.IsAbstract && !m.IsGenericMethod).ToArray();

            // Define delegate types for each of the method signatures
            var delegateMap = new Dictionary<string, Type>();
            foreach (var method in methods)
            {
                var sig = GetMethodSig(method);
                if (delegateMap.ContainsKey(sig))
                    continue;
                var delegateType = CreateDelegateType(moduleBuilder, method);
                delegateMap.Add(sig, delegateType);
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
                var baseConstructor = typeof(T).GetConstructor(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);
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
                il.Emit(OpCodes.Callvirt, typeof(INativeLibImporter).GetMethod("LoadLibrary"));
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
                    il.Emit(OpCodes.Call, typeof(System.Type).GetMethod("GetTypeFromHandle")); // typeof()
                    il.Emit(OpCodes.Call, typeof(U).GetMethod("LoadFunc")); // U.LoadFunc()
                    il.Emit(OpCodes.Isinst, delegates[i].DelegateType); // as <delegate type>
                    il.Emit(OpCodes.Stfld, fields[i]);
                }

                // End of constructor
                il.Emit(OpCodes.Ret);
            }

            // Create destructor
            var destructor = typeBuilder.DefineMethod("Finalize", MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig);
            {
                var baseDestructor = typeof(T).GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance);
                var il = destructor.GetILGenerator();
                var end = il.DefineLabel();

                il.BeginExceptionBlock();
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldfld, field_importer); // .importer
                il.Emit(OpCodes.Ldarg_0); // this
                il.Emit(OpCodes.Ldfld, field_libraryHandle); // .libraryHandle
                il.Emit(OpCodes.Callvirt, typeof(INativeLibImporter).GetMethod("FreeLibrary")); // INativeLibImporter::FreeLibrary()
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
                il.Emit(OpCodes.Callvirt, delegates[i].DelegateType.GetMethod("Invoke"));
                il.Emit(OpCodes.Ret);
            }

            var type = typeBuilder.CreateType();

            assemblyBuilder.Save("dynamic.dll");

            var construct = type.GetConstructor(new Type[] { typeof(INativeLibImporter), typeof(string) });
            var obj = construct.Invoke(new object[] { importer, name });
            var t = obj as T;
            return t;
        }

        private static string GetMethodSig(MethodInfo m)
        {
            return string.Join("_", Enumerable.Repeat(m.ReturnType.Name, 1).Concat(m.GetParameters().Select(p => p.ParameterType.Name)));
        }

        private static Type CreateDelegateType(ModuleBuilder moduleBuilder, MethodInfo methodTemplate)
        {
            var sig = GetMethodSig(methodTemplate);
            var typeBuilder = moduleBuilder.DefineType(sig, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.Sealed, typeof(System.MulticastDelegate));
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[] { typeof(Object), typeof(IntPtr) });
            constructor.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var parameters = methodTemplate.GetParameters().Select(pi => pi.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, methodTemplate.ReturnType, parameters);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            return typeBuilder.CreateType();
        }

    }


}

