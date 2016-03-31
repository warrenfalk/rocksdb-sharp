using System;

namespace RocksDbSharp
{
    /*
    Configure options for a RocksDb store.

    Note on SetXXX() syntax:
       Why not syntax like new Options { XXX = ... } instead?  Two reasons
       1. The rocksdb C API does not support reading the options and so a class with properties is not an appropriate representation
       2. The API functions are named as imperatives and don't always begin with "set" so one like "OptimizeLevelStyleCompaction" wouldn't work right
    */
    public abstract class OptionsHandle : IDisposable, IRocksDbHandle
    {
        public IntPtr Handle { get; private set; }

        public OptionsHandle()
        {
            this.Handle = Native.Instance.rocksdb_options_create();
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_options_destroy(Handle);
                Handle = IntPtr.Zero;
            }
        }
    }
}
