using System;

namespace RocksDbSharp
{
    public class WriteOptions : IDisposable, IRocksDbHandle
    {
        private IntPtr handle;

        public WriteOptions()
        {
            handle = Native.rocksdb_writeoptions_create();
        }

        public IntPtr Handle { get { return handle; } }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.rocksdb_writeoptions_destroy(handle);
                handle = IntPtr.Zero;
            }
        }

        public WriteOptions SetSync(bool value)
        {
            Native.rocksdb_writeoptions_set_sync(handle, value);
            return this;
        }

        public WriteOptions DisableWal(int disable)
        {
            Native.rocksdb_writeoptions_disable_WAL(handle, disable);
            return this;
        }


    }
}
