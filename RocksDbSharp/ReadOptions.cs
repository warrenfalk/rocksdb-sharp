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

        /// <summary>
        /// Enforce that the iterator only iterates over the same prefix as the seek.
        /// This option is effective only for prefix seeks, i.e. prefix_extractor is
        /// non-null for the column family and total_order_seek is false.  Unlike
        /// iterate_upper_bound, prefix_same_as_start only works within a prefix
        /// but in both directions.
        /// Default: false
        /// </summary>
        /// <param name="prefixSameAsStart"></param>
        /// <returns></returns>
        public ReadOptions SetPrefixSameAsStart(bool prefixSameAsStart)
        {
            Native.Instance.rocksdb_readoptions_set_prefix_same_as_start(Handle, prefixSameAsStart);
            return this;
        }

        public unsafe ReadOptions SetIterateUpperBound(byte* key, ulong keylen)
        {
            UIntPtr klen = (UIntPtr)keylen;
            Native.Instance.rocksdb_readoptions_set_iterate_upper_bound(Handle, key, klen);
            return this;
        }

        public ReadOptions SetIterateUpperBound(byte[] key, ulong keyLen)
        {
            if (iterateUpperBound != IntPtr.Zero)
                Marshal.FreeHGlobal(iterateUpperBound);
            iterateUpperBound = Marshal.AllocHGlobal(key.Length);
            Marshal.Copy(key, 0, iterateUpperBound, key.Length);
            UIntPtr klen = (UIntPtr)keyLen;
            Native.Instance.rocksdb_readoptions_set_iterate_upper_bound(Handle, iterateUpperBound, klen);
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
            UIntPtr readaheadSize = (UIntPtr)size;
            Native.Instance.rocksdb_readoptions_set_readahead_size(Handle, readaheadSize);
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
