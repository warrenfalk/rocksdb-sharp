using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Transitional;

namespace RocksDbSharp
{
    public class WriteBatchWithIndex : IWriteBatch
    {
        private IntPtr handle;
        private Encoding defaultEncoding = Encoding.UTF8;

        public WriteBatchWithIndex(ulong reservedBytes = 0, bool overwriteKeys = true)
            : this(Native.Instance.rocksdb_writebatch_wi_create((UIntPtr)reservedBytes, overwriteKeys))
        {
        }

        private WriteBatchWithIndex(IntPtr handle)
        {
            this.handle = handle;
        }

        public IntPtr Handle { get { return handle; } }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_writebatch_wi_destroy(handle);
#endif
                handle = IntPtr.Zero;
            }
        }

        public WriteBatchWithIndex Clear()
        {
            Native.Instance.rocksdb_writebatch_wi_clear(handle);
            return this;
        }

        public int Count()
        {
            return Native.Instance.rocksdb_writebatch_wi_count(handle);
        }

        public Iterator CreateIteratorWithBase(Iterator baseIterator, ColumnFamilyHandle cf = null)
        {
            var handle = cf == null
                ? Native.Instance.rocksdb_writebatch_wi_create_iterator_with_base(Handle, baseIterator.Handle)
                : Native.Instance.rocksdb_writebatch_wi_create_iterator_with_base_cf(Handle, baseIterator.Handle, cf.Handle);
            return new Iterator(handle);
        }

        public string Get(string key, ColumnFamilyHandle cf = null, OptionsHandle options = null, Encoding encoding = null)
        {
            return Native.Instance.rocksdb_writebatch_wi_get_from_batch(Handle, (options ?? RocksDb.DefaultOptions).Handle, key, cf, encoding ?? defaultEncoding);
        }

        public byte[] Get(byte[] key, ColumnFamilyHandle cf = null, OptionsHandle options = null)
        {
            return Get(key, (ulong)key.GetLongLength(0), cf, options);
        }

        public byte[] Get(byte[] key, ulong keyLength, ColumnFamilyHandle cf = null, OptionsHandle options = null)
        {
            return Native.Instance.rocksdb_writebatch_wi_get_from_batch(Handle, (options ?? RocksDb.DefaultOptions).Handle, key, keyLength, cf);
        }

        public ulong Get(byte[] key, byte[] buffer, ulong offset, ulong length, ColumnFamilyHandle cf = null, OptionsHandle options = null)
        {
            return Get(key, (ulong)key.GetLongLength(0), buffer, offset, length, cf, options);
        }

        public ulong Get(byte[] key, ulong keyLength, byte[] buffer, ulong offset, ulong length, ColumnFamilyHandle cf = null, OptionsHandle options = null)
        {
            unsafe
            {
                var ptr = Native.Instance.rocksdb_writebatch_wi_get_from_batch(Handle, (options ?? RocksDb.DefaultOptions).Handle, key, keyLength, out ulong valLength, cf);
                valLength = Math.Min(length, valLength);
                Marshal.Copy(ptr, buffer, (int)offset, (int)valLength);
                Native.Instance.rocksdb_free(ptr);
                return valLength;
            }
        }

        public string Get(RocksDb db, string key, ColumnFamilyHandle cf = null, ReadOptions options = null, Encoding encoding = null)
        {
            return Native.Instance.rocksdb_writebatch_wi_get_from_batch_and_db(Handle, db.Handle, (options ?? RocksDb.DefaultReadOptions).Handle, key, cf, encoding ?? defaultEncoding);
        }

        public byte[] Get(RocksDb db, byte[] key, ColumnFamilyHandle cf = null, ReadOptions options = null)
        {
            return Get(db, key, (ulong)key.GetLongLength(0), cf, options);
        }

        public byte[] Get(RocksDb db, byte[] key, ulong keyLength, ColumnFamilyHandle cf = null, ReadOptions options = null)
        {
            return Native.Instance.rocksdb_writebatch_wi_get_from_batch_and_db(Handle, db.Handle, (options ?? RocksDb.DefaultReadOptions).Handle, key, keyLength, cf);
        }

        public ulong Get(RocksDb db, byte[] key, byte[] buffer, ulong offset, ulong length, ColumnFamilyHandle cf = null, ReadOptions options = null)
        {
            return Get(db, key, (ulong)key.GetLongLength(0), buffer, offset, length, cf, options);
        }

        public ulong Get(RocksDb db, byte[] key, ulong keyLength, byte[] buffer, ulong offset, ulong length, ColumnFamilyHandle cf = null, ReadOptions options = null)
        {
            unsafe
            {
                var ptr = Native.Instance.rocksdb_writebatch_wi_get_from_batch_and_db(Handle, db.Handle, (options ?? RocksDb.DefaultReadOptions).Handle, key, keyLength, out ulong valLength, cf);
                valLength = Math.Min(length, valLength);
                Marshal.Copy(ptr, buffer, (int)offset, (int)valLength);
                Native.Instance.rocksdb_free(ptr);
                return valLength;
            }
        }

        public Iterator NewIterator(Iterator baseIterator, ColumnFamilyHandle cf = null)
        {
            IntPtr iteratorHandle = cf == null
                ? Native.Instance.rocksdb_writebatch_wi_create_iterator_with_base(Handle, baseIterator.Handle)
                : Native.Instance.rocksdb_writebatch_wi_create_iterator_with_base_cf(Handle, baseIterator.Handle, cf.Handle);
            baseIterator.Detach();
            // Note: passing in base iterator here only to ensure that it is not collected before the iterator
            return new Iterator(iteratorHandle);
        }

        public WriteBatchWithIndex Put(string key, string val, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = defaultEncoding;
            Native.Instance.rocksdb_writebatch_wi_put(handle, key, val, encoding);
            return this;
        }

        public WriteBatchWithIndex Put(byte[] key, byte[] val, ColumnFamilyHandle cf = null)
        {
            return Put(key, (ulong)key.Length, val, (ulong)val.Length, cf);
        }

        public WriteBatchWithIndex Put(byte[] key, ulong klen, byte[] val, ulong vlen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_put(handle, key, (UIntPtr)klen, val, (UIntPtr)vlen);
            else
                Native.Instance.rocksdb_writebatch_wi_put_cf(handle, cf.Handle, key, (UIntPtr)klen, val, (UIntPtr)vlen);
            return this;
        }

        public unsafe void Put(byte* key, ulong klen, byte* val, ulong vlen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_put(handle, key, (UIntPtr)klen, val, (UIntPtr)vlen);
            else
                Native.Instance.rocksdb_writebatch_wi_put_cf(handle, cf.Handle, key, (UIntPtr)klen, val, (UIntPtr)vlen);
        }

        public WriteBatchWithIndex Putv(int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
        {
            Native.Instance.rocksdb_writebatch_wi_putv(handle, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
            return this;
        }

        public WriteBatchWithIndex PutvCf(IntPtr columnFamily, int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
        {
            Native.Instance.rocksdb_writebatch_wi_putv_cf(handle, columnFamily, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
            return this;
        }

        public WriteBatchWithIndex Merge(byte[] key, ulong klen, byte[] val, ulong vlen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_merge(handle, key, (UIntPtr)klen, val, (UIntPtr)vlen);
            else
                Native.Instance.rocksdb_writebatch_wi_merge_cf(handle, cf.Handle, key, (UIntPtr)klen, val, (UIntPtr)vlen);
            return this;
        }

        public unsafe void Merge(byte* key, ulong klen, byte* val, ulong vlen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_merge(handle, key, (UIntPtr)klen, val, (UIntPtr)vlen);
            else
                Native.Instance.rocksdb_writebatch_wi_merge_cf(handle, cf.Handle, key, (UIntPtr)klen, val, (UIntPtr)vlen);
        }

        public WriteBatchWithIndex MergeCf(IntPtr columnFamily, byte[] key, ulong klen, byte[] val, ulong vlen)
        {
            Native.Instance.rocksdb_writebatch_wi_merge_cf(handle, columnFamily, key, (UIntPtr)klen, val, (UIntPtr)vlen);
            return this;
        }

        public unsafe void MergeCf(IntPtr columnFamily, byte* key, ulong klen, byte* val, ulong vlen)
        {
            Native.Instance.rocksdb_writebatch_wi_merge_cf(handle, columnFamily, key, (UIntPtr)klen, val, (UIntPtr)vlen);
        }

        public WriteBatchWithIndex Mergev(int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
        {
            Native.Instance.rocksdb_writebatch_wi_mergev(handle, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
            return this;
        }

        public WriteBatchWithIndex MergevCf(IntPtr columnFamily, int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
        {
            Native.Instance.rocksdb_writebatch_wi_mergev_cf(handle, columnFamily, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
            return this;
        }

        public WriteBatchWithIndex Delete(byte[] key, ColumnFamilyHandle cf = null)
        {
            return Delete(key, (ulong)key.Length, cf);
        }

        public WriteBatchWithIndex Delete(byte[] key, ulong klen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_delete(handle, key, (UIntPtr)klen);
            else
                Native.Instance.rocksdb_writebatch_wi_delete_cf(handle, cf.Handle, key, (UIntPtr)klen);
            return this;
        }

        public unsafe void Delete(byte* key, ulong klen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_delete(handle, key, (UIntPtr)klen);
            else
                Native.Instance.rocksdb_writebatch_wi_delete_cf(handle, cf.Handle, key, (UIntPtr)klen);
        }

        public unsafe void Deletev(int numKeys, IntPtr keysList, IntPtr keysListSizes, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_deletev(handle, numKeys, keysList, keysListSizes);
            else
                Native.Instance.rocksdb_writebatch_wi_deletev_cf(handle, cf.Handle, numKeys, keysList, keysListSizes);
        }

        public WriteBatchWithIndex DeleteRange(byte[] startKey, ulong sklen, byte[] endKey, ulong eklen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_delete_range(handle, startKey, (UIntPtr)sklen, endKey, (UIntPtr)eklen);
            else
                Native.Instance.rocksdb_writebatch_wi_delete_range_cf(handle, cf.Handle, startKey, (UIntPtr)sklen, endKey, (UIntPtr)eklen);
            return this;
        }

        public unsafe void DeleteRange(byte* startKey, ulong sklen, byte* endKey, ulong eklen, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_delete_range(handle, startKey, (UIntPtr)sklen, endKey, (UIntPtr)eklen);
            else
                Native.Instance.rocksdb_writebatch_wi_delete_range_cf(handle, cf.Handle, startKey, (UIntPtr)sklen, endKey, (UIntPtr)eklen);
        }

        public unsafe void DeleteRangev(int numKeys, IntPtr startKeysList, IntPtr startKeysListSizes, IntPtr endKeysList, IntPtr endKeysListSizes, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_writebatch_wi_delete_rangev(handle, numKeys, startKeysList, startKeysListSizes, endKeysList, endKeysListSizes);
            else
                Native.Instance.rocksdb_writebatch_wi_delete_rangev_cf(handle, cf.Handle, numKeys, startKeysList, startKeysListSizes, endKeysList, endKeysListSizes);
        }

        public WriteBatchWithIndex PutLogData(byte[] blob, ulong len)
        {
            Native.Instance.rocksdb_writebatch_wi_put_log_data(handle, blob, (UIntPtr)len);
            return this;
        }

        public WriteBatchWithIndex Iterate(IntPtr state, PutDelegate put, DeletedDelegate deleted)
        {
            Native.Instance.rocksdb_writebatch_wi_iterate(handle, state, put, deleted);
            return this;
        }

        /// <summary>
        /// Get the write batch as bytes
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return Native.Instance.rocksdb_writebatch_wi_data(handle);
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
            if (Native.Instance.rocksdb_writebatch_wi_data(handle, buffer, 0, size) > 0)
                return buffer;
            return null;
        }

        public void SetSavePoint()
        {
            Native.Instance.rocksdb_writebatch_wi_set_save_point(handle);
        }

        public void RollbackToSavePoint()
        {
            Native.Instance.rocksdb_writebatch_wi_rollback_to_save_point(handle);
        }


        IWriteBatch IWriteBatch.Clear()
            => Clear();
        IWriteBatch IWriteBatch.Put(string key, string val, Encoding encoding)
            => Put(key, val, encoding);
        IWriteBatch IWriteBatch.Put(byte[] key, byte[] val, ColumnFamilyHandle cf)
            => Put(key, val, cf);
        IWriteBatch IWriteBatch.Put(byte[] key, ulong klen, byte[] val, ulong vlen, ColumnFamilyHandle cf)
            => Put(key, klen, val, vlen, cf);
        IWriteBatch IWriteBatch.Putv(int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
            => Putv(numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
        IWriteBatch IWriteBatch.PutvCf(IntPtr columnFamily, int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
            => PutvCf(columnFamily, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
        IWriteBatch IWriteBatch.Merge(byte[] key, ulong klen, byte[] val, ulong vlen, ColumnFamilyHandle cf)
            => Merge(key, klen, val, vlen, cf);
        IWriteBatch IWriteBatch.MergeCf(IntPtr columnFamily, byte[] key, ulong klen, byte[] val, ulong vlen)
            => MergeCf(columnFamily, key, klen, val, vlen);
        IWriteBatch IWriteBatch.Mergev(int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
            => Mergev(numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
        IWriteBatch IWriteBatch.MergevCf(IntPtr columnFamily, int numKeys, IntPtr keysList, IntPtr keysListSizes, int numValues, IntPtr valuesList, IntPtr valuesListSizes)
            => MergevCf(columnFamily, numKeys, keysList, keysListSizes, numValues, valuesList, valuesListSizes);
        IWriteBatch IWriteBatch.Delete(byte[] key, ColumnFamilyHandle cf)
            => Delete(key, cf);
        IWriteBatch IWriteBatch.Delete(byte[] key, ulong klen, ColumnFamilyHandle cf)
            => Delete(key, klen, cf);
        IWriteBatch IWriteBatch.DeleteRange(byte[] startKey, ulong sklen, byte[] endKey, ulong eklen, ColumnFamilyHandle cf)
            => DeleteRange(startKey, sklen, endKey, eklen, cf);
        IWriteBatch IWriteBatch.PutLogData(byte[] blob, ulong len)
            => PutLogData(blob, len);
        IWriteBatch IWriteBatch.Iterate(IntPtr state, PutDelegate put, DeletedDelegate deleted)
            => Iterate(state, put, deleted);
    }
}
