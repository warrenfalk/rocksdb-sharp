using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public class ReadOptions : IDisposable, IRocksDbHandle
    {
        private IntPtr handle;

        public ReadOptions()
        {
            handle = Native.rocksdb_readoptions_create();
        }

        public IntPtr Handle { get { return handle; } }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.rocksdb_readoptions_destroy(handle);
                handle = IntPtr.Zero;
            }
        }
    }
}
