using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public abstract partial class Native
    {
        public static Native Instance;
        
        static Native()
        {
            var subdir = Environment.Is64BitProcess ? "amd64" : "i386";
            if (!Environment.Is64BitProcess && (int)Environment.OSVersion.Platform <= 3)
                throw new RocksDbSharpException("Rocksdb on windows is not supported for 32 bit applications");
            Instance = NativeImport.Auto.Import<Native>("rocksdb", "4.13.4", true);
        }
    }
}
