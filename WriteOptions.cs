using System;

namespace RocksDbSharp
{
    public class WriteOptions
    {
        public WriteOptions()
        {
            Handle = Native.Instance.rocksdb_writeoptions_create();
        }

        public IntPtr Handle { get; protected set; }

        ~WriteOptions()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_writeoptions_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }

        public WriteOptions SetSync(bool value)
        {
            Native.Instance.rocksdb_writeoptions_set_sync(Handle, value);
            return this;
        }

        public WriteOptions DisableWal(int disable)
        {
            Native.Instance.rocksdb_writeoptions_disable_WAL(Handle, disable);
            return this;
        }


    }
}
