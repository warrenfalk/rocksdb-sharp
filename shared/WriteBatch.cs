using System;
using System.Text;

namespace RocksDbSharp
{
    public class WriteBatch : IDisposable
    {
        private IntPtr handle;
        private Encoding defaultEncoding = Encoding.UTF8;

        public WriteBatch()
            : this(Native.Instance.rocksdb_writebatch_create())
        {
        }

        public WriteBatch(byte[] rep, long size = -1)
            : this(Native.Instance.rocksdb_writebatch_create_from(rep, size < 0 ? rep.Length : size))
        {
        }

        private WriteBatch(IntPtr handle)
        {
            this.handle = handle;
        }

        public IntPtr Handle { get { return handle; } }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_writebatch_destroy(handle);
#endif
                handle = IntPtr.Zero;
            }
        }

        public WriteBatch Clear()
        {
            Native.Instance.rocksdb_writebatch_clear(handle);
            return this;
        }

        public int Count()
        {
            return Native.Instance.rocksdb_writebatch_count(handle);
        }

        public WriteBatch Put(string key, string val, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = defaultEncoding;
            Native.Instance.rocksdb_writebatch_put(handle, key, val, encoding);
            return this;
        }

        public WriteBatch Put(byte[] key, byte[] val, ColumnFamilyHandle cf = null)
        {
            return Put(key, (ulong)key.Length, val, (ulong)val.Length, cf);
        }

        public WriteBatch Put(byte[] key, ulong klen, byte[] val, ulong vlen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_put(handle, key, klen, val, vlen);
            else
                Native.Instance.rocksdb_writebatch_put_cf(handle, cf.Handle, key, klen, val, vlen);
            return this;
        }

        public unsafe void Put(byte* key, ulong klen, byte* val, ulong vlen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_put(handle, key, klen, val, vlen);
            else
                Native.Instance.rocksdb_writebatch_put_cf(handle, cf.Handle, key, klen, val, vlen);
        }

        public WriteBatch Putv(int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
        {
            Native.Instance.rocksdb_writebatch_putv(handle, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
            return this;
        }

        public WriteBatch PutvCf(IntPtr columnFamily, int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
        {
            Native.Instance.rocksdb_writebatch_putv_cf(handle, columnFamily, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
            return this;
        }

        public WriteBatch Merge(byte[] key, ulong klen, byte[] val, ulong vlen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_merge(handle, key, klen, val, vlen);
            else
                Native.Instance.rocksdb_writebatch_merge_cf(handle, cf.Handle, key, klen, val, vlen);
            return this;
        }

        public unsafe void Merge(byte* key, ulong klen, byte* val, ulong vlen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_merge(handle, key, klen, val, vlen);
            else
                Native.Instance.rocksdb_writebatch_merge_cf(handle, cf.Handle, key, klen, val, vlen);
        }

        public WriteBatch MergeCf(IntPtr columnFamily, byte[] key, ulong klen, byte[] val, ulong vlen)
        {
            Native.Instance.rocksdb_writebatch_merge_cf(handle, columnFamily, key, klen, val, vlen);
            return this;
        }

        public unsafe void MergeCf(IntPtr columnFamily, byte* key, ulong klen, byte* val, ulong vlen)
        {
            Native.Instance.rocksdb_writebatch_merge_cf(handle, columnFamily, key, klen, val, vlen);
        }

        public WriteBatch Mergev(int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
        {
            Native.Instance.rocksdb_writebatch_mergev(handle, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
            return this;
        }

        public WriteBatch MergevCf(IntPtr columnFamily, int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
        {
            Native.Instance.rocksdb_writebatch_mergev_cf(handle, columnFamily, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
            return this;
        }

        public WriteBatch Delete(byte[] key, ColumnFamilyHandle cf = null)
        {
            return Delete(key, (ulong)key.Length, cf);
        }

        public WriteBatch Delete(byte[] key, ulong klen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_delete(handle, key, klen);
            else
                Native.Instance.rocksdb_writebatch_delete_cf(handle, cf.Handle, key, klen);
            return this;
        }

        public unsafe void Delete(byte* key, ulong klen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_delete(handle, key, klen);
            else
                Native.Instance.rocksdb_writebatch_delete_cf(handle, cf.Handle, key, klen);
        }

        public WriteBatch Deletev(int numKeys, IntPtr keysList, IntPtr keysListSizes)
        {
            Native.Instance.rocksdb_writebatch_deletev(handle, numKeys, keysList, keysListSizes);
            return this;
        }

        public WriteBatch DeletevCf(IntPtr columnFamily, int numKeys, IntPtr keysList, IntPtr keysListSizes)
        {
            Native.Instance.rocksdb_writebatch_deletev_cf(handle, columnFamily, numKeys, keysList, keysListSizes);
            return this;
        }

        public WriteBatch PutLogData(byte[] blob, ulong len)
        {
            Native.Instance.rocksdb_writebatch_put_log_data(handle, blob, len);
            return this;
        }

        public WriteBatch Iterate(IntPtr state, WriteBatchIteratePutCallback put, WriteBatchIterateDeleteCallback deleted)
        {
            Native.Instance.rocksdb_writebatch_iterate(handle, state, put, deleted);
            return this;
        }

        /// <summary>
        /// Get the write batch as bytes
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return Native.Instance.rocksdb_writebatch_data(handle);
        }

        /// <summary>
        /// Get the write batch as bytes
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns>null if size was not large enough to hold the data</returns>
        public byte[] ToBytes(byte[] buffer, int offset = 0, int size = -1)
        {
            if (size < 0)
                size = buffer.Length;
            if (Native.Instance.rocksdb_writebatch_data(handle, buffer, 0, size) > 0)
                return buffer;
            return null;
        }
    }
}
