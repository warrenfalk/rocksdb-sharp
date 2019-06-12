using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Text;
using Transitional;

namespace RocksDbSharp
{
    public class TransactionDb : IDisposable
    {
        public IntPtr Handle { get; private set; }
        internal DbOptions DbOptions { get; private set; }
        internal TransactionDbOptions TDbOptions { get; private set; }
        public string Name { get; private set; }
        internal ReadOptions DefaultReadOptions { get; set; } = new ReadOptions();
        internal WriteOptions DefaultWriteOptions { get; set; } = new WriteOptions();
        internal static Encoding DefaultEncoding => Encoding.UTF8;
        private Dictionary<string, ColumnFamilyHandleInternal> columnFamilies;

        // Managed references to unmanaged resources that need to live at least as long as the db
        internal dynamic References { get; } = new ExpandoObject();

        private TransactionDb(IntPtr h, DbOptions db_options, TransactionDbOptions txn_db_options, dynamic cfOptionsRefs, Dictionary<string, ColumnFamilyHandleInternal> columnFamilies = null)
        {
            Handle = h;
            DbOptions = db_options;
            TDbOptions = txn_db_options;
            References.CfOptions = cfOptionsRefs;
            this.columnFamilies = columnFamilies ?? new Dictionary<string, ColumnFamilyHandleInternal>();
        }

        public void Dispose()
        {
            foreach (var cfh in columnFamilies.Values)
                cfh.Dispose();

            if (Handle != IntPtr.Zero)
                Native.Instance.rocksdb_transactiondb_close(Handle);
            Handle = IntPtr.Zero;
        }

        public Transaction BeginTransaction(WriteOptions wo, TransactionOptions to, Transaction prev = null)
        {
            IntPtr handle = Native.Instance.rocksdb_transaction_begin(Handle, wo.Handle, to.Handle, (prev != null) ? prev.Handle : IntPtr.Zero);

            return new Transaction(handle, wo, to);
        }

        public static TransactionDb Open(DbOptions options, TransactionDbOptions txn_db_options, string path)
        {
            IntPtr db = Native.Instance.rocksdb_transactiondb_open(options.Handle, txn_db_options.Handle, path);
            return new TransactionDb(db, options, txn_db_options, null);
        }

        /// <summary>
        /// Usage:
        /// <code><![CDATA[
        /// using (var cp = db.Checkpoint())
        /// {
        ///     cp.Save("path/to/checkpoint");
        /// }
        /// ]]></code>
        /// </summary>
        /// <returns></returns>
        public Checkpoint Checkpoint()
        {
            var checkpoint = Native.Instance.rocksdb_transactiondb_checkpoint_object_create(Handle);
            return new Checkpoint(checkpoint);
        }

        public Snapshot CreateSnapshot()
        {
            IntPtr snapshotHandle = Native.Instance.rocksdb_transactiondb_create_snapshot(Handle);
            return new Snapshot(Handle, snapshotHandle, () => Native.Instance.rocksdb_transactiondb_release_snapshot(Handle, snapshotHandle));
        }

        public ColumnFamilyHandle CreateColumnFamily(ColumnFamilyOptions cfOptions, string name)
        {
            var cfh = Native.Instance.rocksdb_transactiondb_create_column_family(Handle, cfOptions.Handle, name);
            var cfhw = new ColumnFamilyHandleInternal(cfh);
            columnFamilies.Add(name, cfhw);
            return cfhw;
        }

        public ColumnFamilyHandle GetDefaultColumnFamily()
        {
            return GetColumnFamily(ColumnFamilies.DefaultName);
        }

        public ColumnFamilyHandle GetColumnFamily(string name)
        {
            if (columnFamilies == null)
                throw new RocksDbSharpException("Database not opened for column families");
            return columnFamilies[name];
        }

        public void Put(string key, string value, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null, Encoding encoding = null)
        {
            Native.Instance.rocksdb_transactiondb_put(Handle, (writeOptions ?? DefaultWriteOptions).Handle, key, value, cf, encoding ?? DefaultEncoding);
        }

        public void Put(byte[] key, byte[] value, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Put(key, key.GetLongLength(0), value, value.GetLongLength(0), cf, writeOptions);
        }

        public void Put(byte[] key, long keyLength, byte[] value, long valueLength, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_transactiondb_put(Handle, (writeOptions ?? DefaultWriteOptions).Handle, key, keyLength, value, valueLength, cf);
        }

        public string Get(string key, ColumnFamilyHandle cf = null, ReadOptions readOptions = null, Encoding encoding = null)
        {
            return Native.Instance.rocksdb_transactiondb_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, cf, encoding ?? DefaultEncoding);
        }

        public byte[] Get(byte[] key, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Get(key, key.GetLongLength(0), cf, readOptions);
        }

        public byte[] Get(byte[] key, long keyLength, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Native.Instance.rocksdb_transactiondb_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, keyLength, cf);
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
                var ptr = Native.Instance.rocksdb_transactiondb_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, keyLength, out long valLength, cf);
                if (ptr == IntPtr.Zero)
                    return -1;
                var copyLength = Math.Min(length, valLength);
                Marshal.Copy(ptr, buffer, (int)offset, (int)copyLength);
                Native.Instance.rocksdb_free(ptr);
                return valLength;
            }
        }

        public void Write(WriteBatch writeBatch, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_transactiondb_write(Handle, (writeOptions ?? DefaultWriteOptions).Handle, writeBatch.Handle);
        }

        public void Remove(string key, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_transactiondb_delete(Handle, (writeOptions ?? DefaultWriteOptions).Handle, key, cf);
        }

        public void Remove(byte[] key, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Remove(key, key.Length, cf, writeOptions);
        }

        public void Remove(byte[] key, long keyLength, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_transactiondb_delete(Handle, (writeOptions ?? DefaultWriteOptions).Handle, key, keyLength);
            else
                Native.Instance.rocksdb_transactiondb_delete_cf(Handle, (writeOptions ?? DefaultWriteOptions).Handle, key, keyLength, cf);
        }

        public Iterator NewIterator(ReadOptions readOptions = null)
        {
            IntPtr iteratorHandle = Native.Instance.rocksdb_transactiondb_create_iterator(Handle, (readOptions ?? DefaultReadOptions).Handle);
            // Note: passing in read options here only to ensure that it is not collected before the iterator
            return new Iterator(iteratorHandle, readOptions);
        }
    }
}
