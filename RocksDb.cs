using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RocksDbSharp
{
    public class RocksDb : IDisposable
    {
        private IntPtr handle;
        private ReadOptions defaultReadOptions;
        private WriteOptions defaultWriteOptions;
        private Encoding defaultEncoding;
        private Dictionary<string, ColumnFamilyHandle> columnFamilies;

        private RocksDb(IntPtr handle, Dictionary<string, ColumnFamilyHandle> columnFamilies = null)
        {
            this.handle = handle;
            defaultReadOptions = new ReadOptions();
            defaultWriteOptions = new WriteOptions();
            defaultEncoding = Encoding.UTF8;
            this.columnFamilies = columnFamilies;
        }

        public void Dispose()
        {
            if (columnFamilies != null)
            {
                foreach (var cfh in columnFamilies.Values)
                    cfh.Dispose();
            }
            Native.Instance.rocksdb_close(handle);
        }

        public static RocksDb Open(OptionsHandle options, string path)
        {
            IntPtr db = Native.Instance.rocksdb_open(options.Handle, path);
            return new RocksDb(db);
        }

        public static RocksDb Open(DbOptions options, string path, ColumnFamilies columnFamilies)
        {
            string[] cfnames = columnFamilies.Select(cfd => cfd.Name).ToArray();
            IntPtr[] cfoptions = columnFamilies.Select(cfd => cfd.Options.Handle).ToArray();
            IntPtr[] cfhandles = new IntPtr[cfnames.Length];
            IntPtr errptr;
            IntPtr db = Native.Instance.rocksdb_open_column_families(options.Handle, path, cfnames.Length, cfnames, cfoptions, cfhandles, out errptr);
            var cfHandleMap = new Dictionary<string, ColumnFamilyHandle>();
            foreach (var pair in cfnames.Zip(cfhandles.Select(cfh => new ColumnFamilyHandle(cfh)), (name, cfh) => new { Name = name, Handle = cfh }))
                cfHandleMap.Add(pair.Name, pair.Handle);
            return new RocksDb(db, cfHandleMap);
        }

        public string Get(string key, ColumnFamilyHandle cf = null, ReadOptions readOptions = null, Encoding encoding = null)
        {
            return Native.Instance.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, cf, encoding ?? defaultEncoding);
        }

        public byte[] Get(byte[] key, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Get(key, key.LongLength, cf, readOptions);
        }

        public byte[] Get(byte[] key, long keyLength, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Native.Instance.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, keyLength, cf);
        }

        public long Get(byte[] key, byte[] buffer, long offset, long length, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Get(key, key.LongLength, buffer, offset, length, cf, readOptions);
        }

        public long Get(byte[] key, long keyLength, byte[] buffer, long offset, long length, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            unsafe
            {
                long valLength;
                var ptr = Native.Instance.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, keyLength, out valLength, cf);
                valLength = Math.Min(length, valLength);
                Marshal.Copy(ptr, buffer, (int)offset, (int)valLength);
                Native.Instance.rocksdb_free(ptr);
                return valLength;
            }
        }

        public void Write(WriteBatch writeBatch, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_write(handle, (writeOptions ?? defaultWriteOptions).Handle, writeBatch.Handle);
        }

        public void Remove(string key, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_delete(handle, (writeOptions ?? defaultWriteOptions).Handle, key, cf);
        }

        public void Remove(byte[] key, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Remove(key, key.Length, cf, writeOptions);
        }

        public void Remove(byte[] key, long keyLength, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_delete(handle, (writeOptions ?? defaultWriteOptions).Handle, key, keyLength);
        }

        public void Put(string key, string value, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null, Encoding encoding = null)
        {
            Native.Instance.rocksdb_put(handle, (writeOptions ?? defaultWriteOptions).Handle, key, value, cf, encoding ?? defaultEncoding);
        }

        public void Put(byte[] key, byte[] value, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Put(key, key.LongLength, value, value.LongLength, cf, writeOptions);
        }

        public void Put(byte[] key, long keyLength, byte[] value, long valueLength, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_put(handle, (writeOptions ?? defaultWriteOptions).Handle, key, keyLength, value, valueLength, cf);
        }

        public Iterator NewIterator(ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            IntPtr iteratorHandle = cf == null
                ? Native.Instance.rocksdb_create_iterator(handle, (readOptions ?? defaultReadOptions).Handle)
                : Native.Instance.rocksdb_create_iterator_cf(handle, (readOptions ?? defaultReadOptions).Handle, cf.Handle);
            // Note: passing in read options here only to ensure that it is not collected before the iterator
            return new Iterator(iteratorHandle, readOptions);
        }

        public ColumnFamilyHandle CreateColumnFamily(ColumnFamilyOptions cfOptions, string name)
        {
            var cfh = Native.Instance.rocksdb_create_column_family(handle, cfOptions.Handle, name);
            return new ColumnFamilyHandle(cfh);
        }

        public void DropColumnFamily(string name)
        {
            var cf = GetColumnFamily(name);
            Native.Instance.rocksdb_drop_column_family(handle, cf.Handle);
            columnFamilies.Remove(name);
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
    }
}
