using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    /// <summary>
    /// A Snapshot is an immutable object and can therefore be safely
    /// accessed from multiple threads without any external synchronization.
    /// </summary>
    public class Snapshot : IDisposable
    {
        private IntPtr dbHandle;
        public IntPtr Handle { get; private set; }

        internal Snapshot(IntPtr dbHandle, IntPtr snapshotHandle)
        {
            this.dbHandle = dbHandle;
            Handle = snapshotHandle;
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_release_snapshot(dbHandle, Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }
    }
}
