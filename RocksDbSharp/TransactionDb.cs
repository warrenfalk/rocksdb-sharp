using System;
using System.Text;

namespace RocksDbSharp
{
    public class TransactionDb : IDisposable
    {
        public static TransactionDb Open(DbOptions dbo, TransactionDbOptions tdbo, string name)
        {
            IntPtr handle, err;
            handle = Native.Instance.rocksdb_transactiondb_open(dbo.Handle, tdbo.Handle, name, out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err);

            return new TransactionDb(handle, dbo, tdbo, name);
        }

        public IntPtr Handle { get; private set; }
        internal DbOptions DbOptions { get; private set; }
        internal TransactionDbOptions TDbOptions { get; private set; }
        public string Name { get; private set; }
        ReadOptions DefaultReadOptions { get; set; }
        WriteOptions DefaultWriteOptions { get; set; }

        internal TransactionDb(IntPtr h, DbOptions db_options, TransactionDbOptions tdb_options, string name)
        {
            Handle = h;
            DbOptions = db_options;
            TDbOptions = tdb_options;
            Name = name;
            DefaultReadOptions = new ReadOptions();
            DefaultWriteOptions = new WriteOptions();
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (Handle != IntPtr.Zero)
                Native.Instance.rocksdb_transactiondb_close(Handle);
            Handle = IntPtr.Zero;
        }

        public Transaction BeginTransaction(WriteOptions wo, TransactionOptions to, Transaction prev = null)
        {
            IntPtr handle = Native.Instance.rocksdb_transaction_begin(Handle, wo.Handle, to.Handle, (prev != null) ? prev.Handle : IntPtr.Zero);

            return new Transaction(handle, wo, to);
        }

        public Snapshot CreateSnapshot()
        {
            IntPtr snapshotHandle = Native.Instance.rocksdb_transactiondb_create_snapshot(Handle);
            return new Snapshot(Handle, snapshotHandle, () => Native.Instance.rocksdb_transactiondb_release_snapshot(Handle, snapshotHandle));
        }

         public void Put(string key, string val, WriteOptions wo = null, Encoding enc = null)
        {
            IntPtr err;
            Native.Instance.rocksdb_transactiondb_put(Handle, (wo ?? DefaultWriteOptions).Handle, key, val, out err, enc);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err); 
        }

        public void Put(byte[] key, byte[] val, WriteOptions wo = null)
        {
            IntPtr err;
            Native.Instance.rocksdb_transactiondb_put(Handle, (wo ?? DefaultWriteOptions).Handle, key, new UIntPtr((ulong)key.Length), val, new UIntPtr((ulong)val.Length), out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err); 
        }

        public string Get(string key, ReadOptions o = null, Encoding enc = null)
        {
            IntPtr err;
            var result = Native.Instance.rocksdb_transactiondb_get(Handle, (o ?? DefaultReadOptions).Handle, key, out err, enc);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err); 
            
            return result;
        }

        public byte[] Get(byte[] key, ReadOptions o = null)
        {
            IntPtr err;
            var result = Native.Instance.rocksdb_transactiondb_get(Handle, (o ?? DefaultReadOptions).Handle, key, (ulong)key.Length, out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err); 
            
            return result;
        }

        public void Remove(string key, WriteOptions wo = null, Encoding enc = null)
        {
            IntPtr err;
            Native.Instance.rocksdb_transactiondb_delete(Handle, (wo ?? DefaultWriteOptions).Handle, key, out err, enc);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err);
        }

        public void Remove(byte[] key, WriteOptions wo = null)
        {
            IntPtr err;
            Native.Instance.rocksdb_transactiondb_delete(Handle, (wo ?? DefaultWriteOptions).Handle, key, new UIntPtr((ulong)key.Length), out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err);
        }
    }
}
