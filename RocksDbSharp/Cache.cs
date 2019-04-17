using System;
using System.Collections.Generic;
using System.Text;

namespace RocksDbSharp
{
    public class Cache
    {
        public IntPtr Handle { get; protected set; }

        private Cache(IntPtr handle)
        {
            this.Handle = handle;
        }

        ~Cache()
        {
            if (Handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_cache_destroy(Handle);
                Handle = IntPtr.Zero;
            }
        }

        public static Cache CreateLru(ulong capacity)
        {
            IntPtr handle = Native.Instance.rocksdb_cache_create_lru(new UIntPtr(capacity));
            return new Cache(handle);
        }

        public Cache SetCapacity(ulong capacity)
        {
            Native.Instance.rocksdb_cache_set_capacity(Handle, new UIntPtr(capacity));
            return this;
        }

        public ulong GetUsage()
        {
            return Native.Instance.rocksdb_cache_get_usage(Handle).ToUInt64();
        }

        public ulong GetPinnedUsage()
        {
            return Native.Instance.rocksdb_cache_get_pinned_usage(Handle).ToUInt64();
        }
    }
}
