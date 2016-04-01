using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public abstract partial class Native
    {
        public static Native Instance = NativeImport.Auto.Import<Native>("librocksdb", true);
    }
}
