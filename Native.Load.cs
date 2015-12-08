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
            LoadLibrary();
        }

        public static void LoadLibrary()
        {
            Platform.Load("librocksdb");
        }
    }
}
