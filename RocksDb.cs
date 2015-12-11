using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RocksDbSharp
{
    public class RocksDb : IDisposable, IRocksDbHandle
    {
        private IntPtr handle;
        private ReadOptions defaultReadOptions;
        private WriteOptions defaultWriteOptions;
        private Encoding defaultEncoding;

        private RocksDb(IntPtr handle)
        {
            this.handle = handle;
            defaultReadOptions = new ReadOptions();
            defaultWriteOptions = new WriteOptions();
            defaultEncoding = Encoding.UTF8;
        }

        public void Dispose()
        {
            Native.rocksdb_close(handle);
        }

        IntPtr IRocksDbHandle.Handle { get { return handle; } }

        public static RocksDb Open(Options options, string path)
        {
            IntPtr db = Native.rocksdb_open(options.Handle, path);
            return new RocksDb(db);
        }

        public string Get(string key, ReadOptions readOptions = null, Encoding encoding = null)
        {
            return Native.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, encoding ?? defaultEncoding);
        }

        public byte[] Get(byte[] key, ReadOptions readOptions = null)
        {
            return Get(key, key.LongLength, readOptions);
        }

        public byte[] Get(byte[] key, long keyLength, ReadOptions readOptions = null)
        {
            return Native.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, keyLength);
        }

        public long Get(byte[] key, byte[] buffer, long offset, long length, ReadOptions readOptions = null)
        {
            return Get(key, key.LongLength, buffer, offset, length, readOptions);
        }

        public long Get(byte[] key, long keyLength, byte[] buffer, long offset, long length, ReadOptions readOptions = null)
        {
            unsafe
            {
                long valLength;
                var ptr = Native.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, keyLength, out valLength);
                valLength = Math.Min(length, valLength);
                Marshal.Copy(ptr, buffer, (int)offset, (int)valLength);
                Native.rocksdb_free(ptr);
                return valLength;
            }
        }

        public void Write(WriteBatch writeBatch, WriteOptions writeOptions = null)
        {
            Native.rocksdb_write(handle, (writeOptions ?? defaultWriteOptions).Handle, writeBatch.Handle);
        }

        public void Remove(string key, WriteOptions writeOptions = null)
        {
            Native.rocksdb_delete(handle, (writeOptions ?? defaultWriteOptions).Handle, key);
        }

        public void Remove(byte[] key, WriteOptions writeOptions = null)
        {
            Remove(key, key.Length, writeOptions);
        }

        public void Remove(byte[] key, long keyLength, WriteOptions writeOptions = null)
        {
            Native.rocksdb_delete(handle, (writeOptions ?? defaultWriteOptions).Handle, key, keyLength);
        }

        public void Put(string key, string value, WriteOptions writeOptions = null, Encoding encoding = null)
        {
            Native.rocksdb_put(handle, (writeOptions ?? defaultWriteOptions).Handle, key, value, encoding ?? defaultEncoding);
        }

        public void Put(byte[] key, byte[] value, WriteOptions writeOptions = null)
        {
            Put(key, key.LongLength, value, value.LongLength, writeOptions);
        }

        public void Put(byte[] key, long keyLength, byte[] value, long valueLength, WriteOptions writeOptions = null)
        {
            Native.rocksdb_put(handle, (writeOptions ?? defaultWriteOptions).Handle, key, keyLength, value, valueLength);
        }
    }
}
