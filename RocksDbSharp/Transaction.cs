using System;
using System.Runtime.InteropServices;
using System.Text;
using Transitional;

namespace RocksDbSharp
{
    public class Transaction : IDisposable
    {
        internal ReadOptions DefaultReadOptions { get; set; }
        internal WriteOptions WriteOptions { get; set; }
        internal TransactionOptions Options { get; set; }
        internal static Encoding DefaultEncoding => Encoding.UTF8;

        public IntPtr Handle { get; private set; }

        internal Transaction(IntPtr h, WriteOptions wo, TransactionOptions to)
        {
            Handle = h;
            DefaultReadOptions = new ReadOptions();
            WriteOptions = wo;
            Options = to;
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
                Native.Instance.rocksdb_transaction_destroy(Handle);
            Handle = IntPtr.Zero;
        }

        public void Commit()
        {
            IntPtr err;
            Native.Instance.rocksdb_transaction_commit(Handle, out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err);
        }

        public void Rollback()
        {
            IntPtr err;
            Native.Instance.rocksdb_transaction_rollback(Handle, out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err);
        }

        public void Put(string key, string value, ColumnFamilyHandle cf = null, Encoding encoding = null)
        {
            Native.Instance.rocksdb_transaction_put(Handle, key, value, cf, encoding ?? DefaultEncoding);
        }

        public void Put(byte[] key, byte[] value, ColumnFamilyHandle cf = null)
        {
            Put(key, key.GetLongLength(0), value, value.GetLongLength(0), cf);
        }

        public void Put(byte[] key, long keyLength, byte[] value, long valueLength, ColumnFamilyHandle cf = null)
        {
            Native.Instance.rocksdb_transaction_put(Handle, key, keyLength, value, valueLength, cf);
        }

        public string Get(string key, ColumnFamilyHandle cf = null, ReadOptions readOptions = null, Encoding encoding = null)
        {
            return Native.Instance.rocksdb_transaction_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, cf, encoding ?? DefaultEncoding);
        }

        public byte[] Get(byte[] key, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Get(key, key.GetLongLength(0), cf, readOptions);
        }

        public byte[] Get(byte[] key, long keyLength, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Native.Instance.rocksdb_transaction_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, keyLength, cf);
        }

        /// <summary>
        /// Reads the contents of the database value associated with <paramref name="key"/>, if present, into the supplied
        /// <paramref name="buffer"/> at <paramref name="offset"/> up to <paramref name="length"/> bytes, returning the
        /// length of the value in the database, or -1 if the key is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="cf"></param>
        /// <param name="readOptions"></param>
        /// <returns>The actual length of the database field if it exists, otherwise -1</returns>
        public long Get(byte[] key, byte[] buffer, long offset, long length, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Get(key, key.GetLongLength(0), buffer, offset, length, cf, readOptions);
        }

        /// <summary>
        /// Reads the contents of the database value associated with <paramref name="key"/>, if present, into the supplied
        /// <paramref name="buffer"/> at <paramref name="offset"/> up to <paramref name="length"/> bytes, returning the
        /// length of the value in the database, or -1 if the key is not present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="cf"></param>
        /// <param name="readOptions"></param>
        /// <returns>The actual length of the database field if it exists, otherwise -1</returns>
        public long Get(byte[] key, long keyLength, byte[] buffer, long offset, long length, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            unsafe
            {
                var ptr = Native.Instance.rocksdb_transaction_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, keyLength, out long valLength, cf);
                if (ptr == IntPtr.Zero)
                    return -1;
                var copyLength = Math.Min(length, valLength);
                Marshal.Copy(ptr, buffer, (int)offset, (int)copyLength);
                Native.Instance.rocksdb_free(ptr);
                return valLength;
            }
        }

        public void Remove(string key, ColumnFamilyHandle cf = null)
        {
            Native.Instance.rocksdb_transaction_delete(Handle, key, cf);
        }

        public void Remove(byte[] key, ColumnFamilyHandle cf = null)
        {
            Remove(key, key.Length, cf);
        }

        public void Remove(byte[] key, long keyLength, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_transaction_delete(Handle, key, keyLength);
            else
                Native.Instance.rocksdb_transaction_delete_cf(Handle, key, keyLength, cf);
        }

        public Iterator NewIterator(ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            IntPtr iteratorHandle = cf == null
                ? Native.Instance.rocksdb_transaction_create_iterator(Handle, (readOptions ?? DefaultReadOptions).Handle)
                : Native.Instance.rocksdb_transaction_create_iterator_cf(Handle, (readOptions ?? DefaultReadOptions).Handle, cf.Handle);
            // Note: passing in read options here only to ensure that it is not collected before the iterator
            return new Iterator(iteratorHandle, readOptions);
        }
    }
}
