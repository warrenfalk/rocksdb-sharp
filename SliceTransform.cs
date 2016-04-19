using System;

namespace RocksDbSharp
{
    #if ROCKSDB_SLICETRANSFORM
    public class SliceTransform
    {
        public IntPtr Handle { get; protected set; }

        private SliceTransform(IntPtr handle)
        {
            this.Handle = handle;
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

        ~SliceTransform()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_slicetransform_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }
    }
    #endif
}
