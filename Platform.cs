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

        public static void Load(string libname)
        {
            string subdir;
            string extension;

            switch ((int)Environment.OSVersion.Platform)
            {
                case (int)PlatformID.Win32Windows: // Win9x supported?
                case (int)PlatformID.Win32S: // Win16 NTVDM on Win x86?
                case (int)PlatformID.Win32NT: // Windows NT
                case (int)PlatformID.WinCE:
                    extension = ".dll";
                    subdir = Environment.Is64BitProcess ? "amd64" : "i386";
                    break;
                case (int)PlatformID.Unix:
                    extension = ".so";
                    subdir = Environment.Is64BitProcess ? "amd64" : "i386";
                    break;
                case (int)PlatformID.MacOSX:
                case 128: // Mono Mac
                    extension = ".dylib";
                    subdir = Environment.Is64BitProcess ? "amd64" : "i386";
                    break;
                default:
                    extension = ".dll";
                    subdir = Environment.Is64BitProcess ? "amd64" : "i386";
                    break;
            }

            var path = Path.Combine("native", subdir, libname + extension);
            var handle = LoadLibrary(path);
            if (handle == IntPtr.Zero)
                throw new Exception(string.Format("Unable to load dll at {0}", path));
        }
    }
}
