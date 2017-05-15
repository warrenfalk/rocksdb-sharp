using System;
using System.Collections.Generic;
using System.Text;

namespace RocksDbSharp
{
    public class EnvOptions
    {
        public IntPtr Handle { get; protected set; }

        public EnvOptions()
        {
            Handle = Native.Instance.rocksdb_envoptions_create();
        }

        ~EnvOptions()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_envoptions_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }
    }
}
