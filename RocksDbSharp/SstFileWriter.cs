using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Transitional;

namespace RocksDbSharp
{
    public class SstFileWriter : IDisposable
    {
        public IntPtr Handle { get; protected set; }

        internal dynamic References { get; } = new ExpandoObject();

        public SstFileWriter(EnvOptions envOptions = null, ColumnFamilyOptions ioOptions = null)
        {
            if (envOptions == null)
                envOptions = new EnvOptions();
            var opts = ioOptions ?? new ColumnFamilyOptions();
            References.EnvOptions = envOptions;
            References.IoOptions = ioOptions;
            Handle = Native.Instance.rocksdb_sstfilewriter_create(envOptions.Handle, opts.Handle);
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                var handle = Handle;
                Handle = IntPtr.Zero;
                Native.Instance.rocksdb_sstfilewriter_destroy(handle);
            }
        }

        public void Open(string filename)
        {
            Native.Instance.rocksdb_sstfilewriter_open(Handle, filename);
        }

        public void Add(string key, string val)
        {
            Native.Instance.rocksdb_sstfilewriter_add(Handle, key, val);
        }

        public void Add(byte[] key, byte[] val)
        {
            Native.Instance.rocksdb_sstfilewriter_add(Handle, key, (UIntPtr)key.GetLongLength(0), val, (UIntPtr)val.GetLongLength(0));
        }

        public void Finish()
        {
            Native.Instance.rocksdb_sstfilewriter_finish(Handle);
        }

        public void Put(byte[] key, byte[] val)
        {
            Native.Instance.rocksdb_sstfilewriter_put(Handle, key, (UIntPtr)key.Length, val, (UIntPtr)val.Length);
        }

        public void Merge(byte[] key, byte[] val)
        {
            Native.Instance.rocksdb_sstfilewriter_merge(Handle, key, (UIntPtr)key.Length, val, (UIntPtr)val.Length);
        }

        public void Delete(byte[] key)
        {
            Native.Instance.rocksdb_sstfilewriter_delete(Handle, key, (UIntPtr)key.Length);
        }
    }
}
