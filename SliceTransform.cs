using System;

namespace RocksDbSharp
{
    #if ROCKSDB_SLICETRANSFORM
    public class SliceTransform : IDisposable, IRocksDbHandle
    {
        private IntPtr handle;

        public IntPtr Handle { get { return handle; } }

        private SliceTransform(IntPtr handle)
        {
            this.handle = handle;
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_slicetransform_destroy(handle);
                handle = IntPtr.Zero;
            }
        }

        public static SliceTransform CreateFixedPrefix(/*(size_t)*/ ulong fixed_prefix_length)
        {
            IntPtr handle = Native.Instance.rocksdb_slicetransform_create_fixed_prefix(fixed_prefix_length);
            return new SliceTransform(handle);
        }

        public static SliceTransform CreateNoOp()
        {
            IntPtr handle = Native.Instance.rocksdb_slicetransform_create_noop();
            return new SliceTransform(handle);
        }

    }
    #endif
}
