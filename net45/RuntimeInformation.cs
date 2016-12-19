using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
