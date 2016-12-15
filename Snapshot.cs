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
    public class SnapShot : IDisposable
    {
        private IntPtr db_handle;
        public IntPtr snapshot_handle;

        internal SnapShot(IntPtr db_handle, IntPtr snapshot_handle)
        {
            this.db_handle = db_handle;
            this.snapshot_handle = snapshot_handle;
        }

        public void Dispose()
        {
            if (snapshot_handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_release_snapshot(db_handle, snapshot_handle);
#endif
                snapshot_handle = IntPtr.Zero;
            }
        }
    }
}
