using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Transitional;

namespace RocksDbSharp
{
    public class ReadOptions
    {
        private IntPtr iterateUpperBound;

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
                if (iterateUpperBound != IntPtr.Zero)
                    Marshal.FreeHGlobal(iterateUpperBound);
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

        public ReadOptions SetSnapshot(Snapshot snapshot)
        {
            Native.Instance.rocksdb_readoptions_set_snapshot(Handle, snapshot.Handle);
            return this;
        }

        public unsafe ReadOptions SetIterateUpperBound(byte* key, ulong keylen)
        {
            Native.Instance.rocksdb_readoptions_set_iterate_upper_bound(Handle, key, new UIntPtr(keylen));
            return this;
        }

        public ReadOptions SetIterateUpperBound(byte[] key, ulong keyLen)
        {
            if (iterateUpperBound != IntPtr.Zero)
                Marshal.FreeHGlobal(iterateUpperBound);
            iterateUpperBound = Marshal.AllocHGlobal(key.Length);
            Marshal.Copy(key, 0, iterateUpperBound, key.Length);
            Native.Instance.rocksdb_readoptions_set_iterate_upper_bound(Handle, iterateUpperBound, new UIntPtr(keyLen));
            return this;
        }

        public ReadOptions SetIterateUpperBound(byte[] key)
        {
            return SetIterateUpperBound(key, (ulong)key.GetLongLength(0));
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

        public ReadOptions SetReadaheadSize(ulong size)
        {
            Native.Instance.rocksdb_readoptions_set_readahead_size(Handle, size);
            return this;
        }

        public ReadOptions SetPinData(bool enable)
        {
            Native.Instance.rocksdb_readoptions_set_pin_data(Handle, enable);
            return this;
        }

        public ReadOptions SetTotalOrderSeek(bool enable)
        {
            Native.Instance.rocksdb_readoptions_set_total_order_seek(Handle, enable);
            return this;
        }
    }
}
