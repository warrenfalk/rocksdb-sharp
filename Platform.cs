using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RocksDbSharp
{
    /*
        This class was created for the RocksDbSharp project but should remain generic
        in order to be usable in other similar scenarios.
    
        The purpose of this class is to provide a single interface for loading libraries
        on multiple platforms
    */
    static class Platform
    {
        [DllImport("kernel32", CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("libdl")]
        static extern IntPtr dlsym(IntPtr handle, [MarshalAs(UnmanagedType.LPTStr)] string symbol);

        [DllImport("libdl")]
        static extern IntPtr dlopen(string filename, int flags);

        [DllImport("__Internal")]
        private static extern void mono_dllmap_insert(IntPtr assembly, IntPtr dll, IntPtr func, IntPtr tdll, IntPtr tfunc);

        public static void Load(string libname)
        {
            string subdir;
            string extension;
            Func<string, IntPtr> Load;

            switch ((int)Environment.OSVersion.Platform)
            {
                case (int)PlatformID.Win32Windows: // Win9x supported?
                case (int)PlatformID.Win32S: // Win16 NTVDM on Win x86?
                case (int)PlatformID.Win32NT: // Windows NT
                case (int)PlatformID.WinCE:
                    extension = ".dll";
                    subdir = Environment.Is64BitProcess ? "amd64" : "i386";
                    Load = LoadLibrary;
                    break;
                case (int)PlatformID.Unix:
                    extension = ".so";
                    subdir = Environment.Is64BitProcess ? "amd64" : "i386";
                    Load = MonoLoad;
                    break;
                case (int)PlatformID.MacOSX:
                case 128: // Mono Mac
                    extension = ".dylib";
                    subdir = Environment.Is64BitProcess ? "amd64" : "i386";
                    Load = MonoLoad;
                    break;
                default:
                    extension = ".dll";
                    subdir = Environment.Is64BitProcess ? "amd64" : "i386";
                    Load = LoadLibrary;
                    break;
            }

            var path = Path.Combine("native", subdir, libname + extension);
            var handle = Load(path);
            /*
            if (handle == IntPtr.Zero)
                throw new Exception(string.Format("Unable to load dll at {0}", path));
            */
        }

        public static IntPtr MonoLoad(string path)
        {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "", path);

            string name = Path.GetFileNameWithoutExtension(path);
            IntPtr libraryNamePtr = Marshal.StringToHGlobalAnsi(name);
            IntPtr pathPtr = Marshal.StringToHGlobalAnsi(path);
            mono_dllmap_insert(IntPtr.Zero, libraryNamePtr, IntPtr.Zero, pathPtr, IntPtr.Zero);
            Marshal.FreeHGlobal(libraryNamePtr);
            Marshal.FreeHGlobal(pathPtr);
            return IntPtr.Zero; //dlopen(path, 9);
        }
    }
}
