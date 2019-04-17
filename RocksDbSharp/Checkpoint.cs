using System;
using System.Collections.Generic;
using System.Text;

namespace RocksDbSharp
{
    public class Checkpoint : IDisposable
    {
        public IntPtr Handle { get; }

        public Checkpoint(IntPtr handle)
        {
            Handle = handle;
        }

        public void Save(string checkpointDir, ulong logSizeForFlush = 0)
            => Native.Instance.rocksdb_checkpoint_create(Handle, checkpointDir, logSizeForFlush);

        public void Dispose()
        {
            Native.Instance.rocksdb_checkpoint_object_destroy(Handle);
        }
    }
}
