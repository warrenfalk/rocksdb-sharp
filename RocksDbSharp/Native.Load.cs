using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public abstract partial class Native
    {
        public static Native Instance;
        
        static Native()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X86 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new RocksDbSharpException("Rocksdb on windows is not supported for 32 bit applications");
            Instance = NativeImport.Auto.Import<Native>("rocksdb", "6.4.6", true);
        }

        public Native()
        {
        }
    }
}
