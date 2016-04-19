using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public class ReadOptions
    {
        private byte[] iterateUpperBound;

        public ReadOptions()
        {
            Handle = Native.Instance.rocksdb_readoptions_create();
        }

        public IntPtr Handle { get; protected set; }

        ~ReadOptions()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_readoptions_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }

        public ReadOptions SetVerifyChecksums(bool value)
        {
            Native.Instance.rocksdb_readoptions_set_verify_checksums(Handle, value);
            return this;
        }

        public ReadOptions SetFillCache(bool value)
        {
            Native.Instance.rocksdb_readoptions_set_fill_cache(Handle, value);
            return this;
        }

        public ReadOptions SetSnapshot(IntPtr snapshot)
        {
            Native.Instance.rocksdb_readoptions_set_snapshot(Handle, snapshot);
            return this;
        }

        public unsafe ReadOptions SetIterateUpperBound(byte* key, ulong keylen)
        {
            Native.Instance.rocksdb_readoptions_set_iterate_upper_bound(Handle, key, keylen);
            return this;
        }

        public ReadOptions SetIterateUpperBound(byte[] key, ulong keyLen)
        {
            iterateUpperBound = key; // necessary because the value will not be copied and so may be gone by the time it is needed
            Native.Instance.rocksdb_readoptions_set_iterate_upper_bound(Handle, key, keyLen);
            return this;
        }

        public ReadOptions SetIterateUpperBound(byte[] key)
        {
            iterateUpperBound = key; // necessary because the value will not be copied and so may be gone by the time it is needed
            return SetIterateUpperBound(key, (ulong)key.LongLength);
        }

        public unsafe ReadOptions SetIterateUpperBound(string stringKey, Encoding encoding = null)
        {
            var key = (encoding ?? Encoding.UTF8).GetBytes(stringKey);
            return SetIterateUpperBound(key);
        }

        public ReadOptions SetReadTier(int value)
        {
            Native.Instance.rocksdb_readoptions_set_read_tier(Handle, value);
            return this;
        }

        public ReadOptions SetTailing(bool value)
        {
            Native.Instance.rocksdb_readoptions_set_tailing(Handle, value);
            return this;
        }

    }
}
