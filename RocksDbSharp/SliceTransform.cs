using System;

namespace RocksDbSharp
{
    public class SliceTransform
    {
        public IntPtr Handle { get; protected set; }

        private SliceTransform(IntPtr handle)
        {
            this.Handle = handle;
        }

        public static SliceTransform CreateFixedPrefix(/*(size_t)*/ ulong fixed_prefix_length)
        {
            UIntPtr fixedPrefix = (UIntPtr)fixed_prefix_length;
            IntPtr handle = Native.Instance.rocksdb_slicetransform_create_fixed_prefix(fixedPrefix);
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
                // Commented out until a solution is found to rocksdb issue #1095 (https://github.com/facebook/rocksdb/issues/1095)
                // If you create one of these, use it in an Option which will destroy it when finished
                // Otherwise don't create one or it will leak
                //Native.Instance.rocksdb_slicetransform_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }
    }
}
