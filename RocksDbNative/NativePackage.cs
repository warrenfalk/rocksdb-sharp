using System;

namespace RocksDbSharp
{
    public class NativePackage
    {
        public static string GetCodeBase()
#if NETSTANDARD1_6
            => AppContext.BaseDirectory;
#else
            => AppDomain.CurrentDomain.BaseDirectory;
#endif
    }
}
