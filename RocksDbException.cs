using System;
using System.Runtime.InteropServices;

namespace RocksDbSharp
{
    [Serializable]
    public class RocksDbException : Exception
    {
        public RocksDbException(string message)
            : base(message)
        {
        }

        public RocksDbException(IntPtr errptr)
            : this(Marshal.PtrToStringAnsi(errptr))
        {
            Native.Instance.rocksdb_free(errptr);
        }
    }
}