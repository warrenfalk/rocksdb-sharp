using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public class ColumnFamilyHandle : IDisposable
    {
        public ColumnFamilyHandle(IntPtr handle)
        {
            this.Handle = handle;
        }

        public IntPtr Handle { get; protected set; }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_column_family_handle_destroy(Handle);
                Handle = IntPtr.Zero;
            }
        }
    }
}
