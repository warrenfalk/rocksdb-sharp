using System;

namespace RocksDbSharp
{
    public class TransactionDbOptions
    {
        public IntPtr Handle { get; private set; }

        public TransactionDbOptions()
        {
            Handle = Native.Instance.rocksdb_transactiondb_options_create();
        }

        ~TransactionDbOptions()
        {
            if (Handle != IntPtr.Zero)
                Native.Instance.rocksdb_transactiondb_options_destroy(Handle);
            Handle = IntPtr.Zero;
        }

        public TransactionDbOptions SetMaxNumLocks(long max_num_locks)
        {
            Native.Instance.rocksdb_transactiondb_options_set_max_num_locks(Handle, max_num_locks);
            return this;
        }

        public TransactionDbOptions SetNumStripes(ulong num_stripes)
        {
            Native.Instance.rocksdb_transactiondb_options_set_num_stripes(Handle, new UIntPtr(num_stripes));
            return this;
        }

        public TransactionDbOptions SetTransactionLockTimeout(long txn_lock_timeout)
        {
            Native.Instance.rocksdb_transactiondb_options_set_transaction_lock_timeout(Handle, txn_lock_timeout);
            return this;
        }

        public TransactionDbOptions SetDefaultLockTimeout(long default_lock_timeout)
        {
            Native.Instance.rocksdb_transactiondb_options_set_default_lock_timeout(Handle, default_lock_timeout);
            return this;
        }
        
    }
}
