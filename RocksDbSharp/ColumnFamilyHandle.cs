using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
#pragma warning disable IDE1006 // Naming (missing I) for backward source-compatibility reasons
    public interface ColumnFamilyHandle
#pragma warning restore IDE1006
    {
        IntPtr Handle { get; }
    }

    class ColumnFamilyHandleInternal : ColumnFamilyHandle, IDisposable
    {
        public ColumnFamilyHandleInternal(IntPtr handle)
        {
            this.Handle = handle;
        }

        public IntPtr Handle { get; protected set; }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_column_family_handle_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }
    }
}
