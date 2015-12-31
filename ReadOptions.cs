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
            handle = Native.Instance.rocksdb_readoptions_create();
        }

        public IntPtr Handle { get { return handle; } }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_readoptions_destroy(handle);
                handle = IntPtr.Zero;
            }
        }

        public ReadOptions SetVerifyChecksums(bool value)
        {
            Native.Instance.rocksdb_readoptions_set_verify_checksums(handle, value);
            return this;
        }

        public ReadOptions SetFillCache(bool value)
        {
            Native.Instance.rocksdb_readoptions_set_fill_cache(handle, value);
            return this;
        }

        public ReadOptions SetSnapshot(IntPtr snapshot)
        {
            Native.Instance.rocksdb_readoptions_set_snapshot(handle, snapshot);
            return this;
        }

        public unsafe ReadOptions SetIterateUpperBound(byte* key, ulong keylen)
        {
            Native.Instance.rocksdb_readoptions_set_iterate_upper_bound(handle, key, keylen);
            return this;
        }

        public ReadOptions SetReadTier(int value)
        {
            Native.Instance.rocksdb_readoptions_set_read_tier(handle, value);
            return this;
        }

        public ReadOptions SetTailing(bool value)
        {
            Native.Instance.rocksdb_readoptions_set_tailing(handle, value);
            return this;
        }

    }
}
