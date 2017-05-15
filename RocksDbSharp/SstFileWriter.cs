using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace RocksDbSharp
{
    public class SstFileWriter
    {
        public IntPtr Handle { get; protected set; }

        internal dynamic References { get; } = new ExpandoObject();

        public SstFileWriter(EnvOptions envOptions = null, ColumnFamilyOptions ioOptions = null)
        {
            if (envOptions == null)
                envOptions = new EnvOptions();
            var ioOptionsHandle = ioOptions?.Handle ?? IntPtr.Zero;
            References.EnvOptions = envOptions;
            References.IoOptions = ioOptions;
            Handle = Native.Instance.rocksdb_sstfilewriter_create(envOptions.Handle, ioOptionsHandle);
        }

        ~SstFileWriter()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_sstfilewriter_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }

        public void Open(string filename)
        {
            Native.Instance.rocksdb_sstfilewriter_open(Handle, filename);
        }

        public void Add(string key, string val)
        {
            Native.Instance.rocksdb_sstfilewriter_add(Handle, key, (ulong)key.Length, val, (ulong)val.Length);
        }

        public void Add(byte[] key, byte[] val)
        {
            Native.Instance.rocksdb_sstfilewriter_add(Handle, key, (ulong)key.Length, val, (ulong)val.Length);
        }

        public void Finish()
        {
            Native.Instance.rocksdb_sstfilewriter_finish(Handle);
        }
    }
}
