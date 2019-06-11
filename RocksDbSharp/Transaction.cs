using System;
using System.Text;

namespace RocksDbSharp
{
    public class Transaction : IDisposable
    {
        ReadOptions DefaultReadOptions { get; set; }
        WriteOptions DefaultWriteOptions { get; set; }
        TransactionOptions Options { get; set; }

        public IntPtr Handle { get; private set; }

        internal Transaction(IntPtr h, WriteOptions wo, TransactionOptions to)
        {
            Handle = h;
            DefaultReadOptions = new ReadOptions();
            DefaultWriteOptions = wo;
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

        public void Put(string key, string val, Encoding enc = null)
        {
            IntPtr err;
            Native.Instance.rocksdb_transaction_put(Handle, key, val, out err, enc);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err); 
        }

        public void Put(byte[] key, byte[] val)
        {
            IntPtr err;
            Native.Instance.rocksdb_transaction_put(Handle, key, new UIntPtr((ulong)key.Length), val, new UIntPtr((ulong)val.Length), out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err); 
        }

        public string Get(string key, ReadOptions o = null, Encoding enc = null)
        {
            IntPtr err;
            var result = Native.Instance.rocksdb_transaction_get(Handle, (o ?? DefaultReadOptions).Handle, key, out err, enc);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err); 
            
            return result;
        }

        public byte[] Get(byte[] key, ReadOptions o = null)
        {
            IntPtr err;
            var result = Native.Instance.rocksdb_transaction_get(Handle, (o ?? DefaultReadOptions).Handle, key, (ulong)key.Length, out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err); 
            
            return result;
        }

        public void Remove(string key, Encoding enc = null)
        {
            IntPtr err;
            Native.Instance.rocksdb_transaction_delete(Handle, key, out err, enc);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err);
        }

        public void Remove(byte[] key)
        {
            IntPtr err;
            Native.Instance.rocksdb_transaction_delete(Handle, key, new UIntPtr((ulong)key.Length), out err);
            if (err != IntPtr.Zero)
                throw new RocksDbException(err);
        }

        public Iterator NewIterator(ReadOptions readOptions = null)
        {
            IntPtr iteratorHandle = Native.Instance.rocksdb_transaction_create_iterator(Handle, (readOptions ?? DefaultReadOptions).Handle);
            return new Iterator(iteratorHandle, readOptions);
        }
    }
}
