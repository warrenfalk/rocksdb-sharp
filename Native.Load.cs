using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    static partial class Native
    {
        static Native()
        {
            // Note: this works in Windows, but not Mono because mono looks up the unmanaged lib path before this static constructor completes
            // still working on a solution
            LoadLibrary();
        }

        public static void LoadLibrary()
        {
            Platform.Load("librocksdb");
        }
    }
}
