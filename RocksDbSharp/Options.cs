using System;
using System.Dynamic;

namespace RocksDbSharp
{
    /*
    Configure options for a RocksDb store.

    Note on SetXXX() syntax:
       Why not syntax like new Options { XXX = ... } instead?  Two reasons
       1. The rocksdb C API does not support reading the options and so a class with properties is not an appropriate representation
       2. The API functions are named as imperatives and don't always begin with "set" so one like "OptimizeLevelStyleCompaction" wouldn't work right
    */
    public abstract class OptionsHandle
    {
        // The following exists only to retain a reference to those types which are used in-place by rocksdb
        // and not copied (or reference things that are used in-place).  The idea is to have managed references
        // track the behavior of the unmanaged reference as much as possible.  This prevents access violations
        // when the garbage collector cleans up the last managed reference
        internal dynamic References { get; } = new ExpandoObject();

        public IntPtr Handle { get; private set; }

        public OptionsHandle()
        {
            this.Handle = Native.Instance.rocksdb_options_create();
        }

        ~OptionsHandle()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_options_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }
    }
}
