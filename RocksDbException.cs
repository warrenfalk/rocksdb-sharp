using System;
using System.Runtime.InteropServices;

namespace RocksDbSharp
{
    [Serializable]
    public class RocksDbException : RocksDbSharpException
    {
        public RocksDbException(IntPtr errptr)
            : base(Marshal.PtrToStringAnsi(errptr))
        {
            Native.Instance.rocksdb_free(errptr);
        }
    }
}