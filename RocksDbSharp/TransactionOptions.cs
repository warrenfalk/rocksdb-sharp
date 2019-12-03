using System;

namespace RocksDbSharp
{
    public class TransactionOptions
    {
        public IntPtr Handle { get; private set; }

        public TransactionOptions()
        {
            Handle = Native.Instance.rocksdb_transaction_options_create();
        }

        ~TransactionOptions()
        {
            if (Handle != IntPtr.Zero)
                Native.Instance.rocksdb_transaction_options_destroy(Handle);
            Handle = IntPtr.Zero;
        }

        public TransactionOptions SetSetSnapshot(bool v)
        {
            Native.Instance.rocksdb_transaction_options_set_set_snapshot(Handle, v);
            return this;
        }

        public TransactionOptions SetDeadlockDetect(bool v)
        {
            Native.Instance.rocksdb_transaction_options_set_deadlock_detect(Handle, v);
            return this;
        }

        public TransactionOptions SetLockTimeout(long timeout)
        {
            Native.Instance.rocksdb_transaction_options_set_lock_timeout(Handle, timeout);
            return this;
        }

        public TransactionOptions SetExpiration(long expiration)
        {
            Native.Instance.rocksdb_transaction_options_set_expiration(Handle, expiration);
            return this;
        }

        public TransactionOptions SetDeadlockDetectDepth(long depth)
        {
            Native.Instance.rocksdb_transaction_options_set_deadlock_detect_depth(Handle, depth);
            return this;
        }

        public TransactionOptions SetMaxWriteBatchSize(ulong size)
        {
            Native.Instance.rocksdb_transaction_options_set_max_write_batch_size(Handle, new UIntPtr(size));
            return this;
        }
    }
}   
