using System;
using System.IO;
using System.Reflection;

namespace RocksDbSharp
{
    public class NativePackage
    {
        public static string GetCodeBase()
        {
#if NETSTANDARD1_6
            var assemblyLocation = typeof(NativePackage).GetTypeInfo().Assembly.Location;
            var assemblyDir = Path.GetDirectoryName(assemblyLocation);
            return
                Directory.Exists(Path.Combine(assemblyDir, "runtimes")) ? assemblyDir :
                Directory.Exists(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(assemblyDir)), "runtimes")) ? Path.GetDirectoryName(Path.GetDirectoryName(assemblyDir)) :
                assemblyDir;
#else
            return AppDomain.CurrentDomain.BaseDirectory;
#endif
        }
    }
}
