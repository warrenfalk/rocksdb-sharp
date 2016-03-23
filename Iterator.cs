using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public class Iterator : IDisposable, IRocksDbHandle
    {
        private IntPtr handle;

        internal Iterator(IntPtr handle)
        {
            this.handle = handle;
        }

        public IntPtr Handle { get { return handle; } }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_iter_destroy(handle);
                handle = IntPtr.Zero;
            }
        }

        public bool Valid()
        {
            return Native.Instance.rocksdb_iter_valid(handle);
        }

        public Iterator SeekToFirst()
        {
            Native.Instance.rocksdb_iter_seek_to_first(handle);
            return this;
        }

        public Iterator SeekToLast()
        {
            Native.Instance.rocksdb_iter_seek_to_last(handle);
            return this;
        }

        public unsafe Iterator Seek(byte *key, ulong klen)
        {
            Native.Instance.rocksdb_iter_seek(handle, key, klen);
            return this;
        }

        public Iterator Seek(byte[] key)
        {
            return Seek(key, (ulong)key.LongLength);
        }

        public Iterator Seek(byte[] key, ulong klen)
        {
            Native.Instance.rocksdb_iter_seek(handle, key, klen);
            return this;
        }

        public Iterator Seek(string key)
        {
            Native.Instance.rocksdb_iter_seek(handle, key);
            return this;
        }

        public Iterator Next()
        {
            Native.Instance.rocksdb_iter_next(handle);
            return this;
        }

        public Iterator Prev()
        {
            Native.Instance.rocksdb_iter_prev(handle);
            return this;
        }

        public byte[] Key()
        {
            return Native.Instance.rocksdb_iter_key(handle);
        }

        public byte[] Value()
        {
            return Native.Instance.rocksdb_iter_value(handle);
        }

        public string StringKey()
        {
            return Native.Instance.rocksdb_iter_key_string(handle);
        }

        public string StringValue()
        {
            return Native.Instance.rocksdb_iter_value_string(handle);
        }

        // TODO: figure out how to best implement rocksdb_iter_get_error
    }
}
