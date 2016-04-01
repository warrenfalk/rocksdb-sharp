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
            if (errptr != IntPtr.Zero)
            {
                // The following is a kludge because as far as I can tell, the interface for creating column families
                // in rocksdb is fundamentaly broken, so here we attempt to see if it failed because we specified a column family that doesn't exist
                // if so, and if we have create_if_missing specified, then we'll go ahead and create those and then attempt to open again
                // This is a deviation from the C++ interface, but I haven't currently found a cleaner way.
                string errmsg = Marshal.PtrToStringAnsi(errptr);
                if (errmsg.StartsWith("Invalid argument: Column family not found:") && options.CreateIfMissing && columnFamilies != null && columnFamilies.Count() > 1)
                {
                    // the db has now been created if it didn't exist, or it existed already but lacked a column family we've specified
                    // list the existing column families and re-open the database specifying just those
                    // then go ahead and create the column families that don't exist
                    var names = Native.Instance.rocksdb_list_column_families(options.Handle, path);
                    var existing = new HashSet<string>(names);
                    // sort existing to top
                    var families =
                        columnFamilies.Where(cfd => existing.Contains(cfd.Name))
                        .Concat(columnFamilies.Where(cfd => !existing.Contains(cfd.Name)))
                        .ToArray();
                    cfnames = families.Select(cfd => cfd.Name).ToArray();
                    cfoptions = families.Select(cfd => cfd.Options.Handle).ToArray();
                    int existingCount = existing.Count;
                    errptr = IntPtr.Zero;
                    db = Native.Instance.rocksdb_open_column_families(options.Handle, path, existingCount, cfnames, cfoptions, cfhandles, out errptr);
                    if (errptr != IntPtr.Zero)
                        throw new RocksDbException(errptr);
                    for (var i = existingCount; i < cfnames.Length; i++)
                    {
                        cfhandles[i] = Native.Instance.rocksdb_create_column_family(db, cfoptions[i], cfnames[i]);
                    }
                }
                else
                {
                    throw new RocksDbException(errptr);
                }
            }
            var cfHandleMap = new Dictionary<string, ColumnFamilyHandle>();
            foreach (var pair in cfnames.Zip(cfhandles.Select(cfh => new ColumnFamilyHandle(cfh)), (name, cfh) => new { Name = name, Handle = cfh }))
                cfHandleMap.Add(pair.Name, pair.Handle);
            return new RocksDb(db, cfHandleMap);
        }

        public string Get(string key, ReadOptions readOptions = null, Encoding encoding = null)
        {
            return Native.Instance.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, encoding ?? defaultEncoding);
        }

        public byte[] Get(byte[] key, ReadOptions readOptions = null)
        {
            return Get(key, key.LongLength, readOptions);
        }

        public byte[] Get(byte[] key, long keyLength, ReadOptions readOptions = null)
        {
            return Native.Instance.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, keyLength);
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
                var ptr = Native.Instance.rocksdb_get(handle, (readOptions ?? defaultReadOptions).Handle, key, keyLength, out valLength);
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

        public void Remove(string key, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_delete(handle, (writeOptions ?? defaultWriteOptions).Handle, key);
        }

        public void Remove(byte[] key, WriteOptions writeOptions = null)
        {
            Remove(key, key.Length, writeOptions);
        }

        public void Remove(byte[] key, long keyLength, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_delete(handle, (writeOptions ?? defaultWriteOptions).Handle, key, keyLength);
        }

        public void Put(string key, string value, WriteOptions writeOptions = null, Encoding encoding = null)
        {
            Native.Instance.rocksdb_put(handle, (writeOptions ?? defaultWriteOptions).Handle, key, value, encoding ?? defaultEncoding);
        }

        public void Put(byte[] key, byte[] value, WriteOptions writeOptions = null)
        {
            Put(key, key.LongLength, value, value.LongLength, writeOptions);
        }

        public void Put(byte[] key, long keyLength, byte[] value, long valueLength, WriteOptions writeOptions = null)
        {
            Native.Instance.rocksdb_put(handle, (writeOptions ?? defaultWriteOptions).Handle, key, keyLength, value, valueLength);
        }

        public Iterator NewIterator(ReadOptions readOptions = null)
        {
            IntPtr iteratorHandle = Native.Instance.rocksdb_create_iterator(handle, (readOptions ?? defaultReadOptions).Handle);
            // Note: passing in read options here only to ensure that it is not collected before the iterator
            return new Iterator(iteratorHandle, readOptions);
        }

        public ColumnFamilyHandle CreateColumnFamily(ColumnFamilyOptions cfOptions, string name)
        {
            var cfh = Native.Instance.rocksdb_create_column_family(handle, cfOptions.Handle, name);
            return new ColumnFamilyHandle(cfh);
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
