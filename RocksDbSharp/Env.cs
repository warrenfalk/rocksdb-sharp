using System;
using System.Collections.Generic;
using System.Text;

namespace RocksDbSharp
{
    public class Env
    {
        public IntPtr Handle { get; protected set; }

        private Env(IntPtr handle)
        {
            Handle = handle;
        }

        public static Env CreateDefaultEnv()
        {
            return new Env(Native.Instance.rocksdb_create_default_env());
        }

        public static Env CreateMemEnv()
        {
            return new Env(Native.Instance.rocksdb_create_mem_env());
        }

        public Env SetBackgroundThreads(int value)
        {
            Native.Instance.rocksdb_env_set_background_threads(Handle, value);
            return this;
        }

        public Env SetHighPriorityBackgroundThreads(int value)
        {
            Native.Instance.rocksdb_env_set_high_priority_background_threads(Handle, value);
            return this;
        }

        public void JoinAllThreads()
        {
            Native.Instance.rocksdb_env_join_all_threads(Handle);
        }

        ~Env()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_env_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }
    }
}
