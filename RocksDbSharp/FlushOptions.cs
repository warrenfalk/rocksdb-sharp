using System;
using System.Collections.Generic;
using System.Text;

namespace RocksDbSharp
{
    public class FlushOptions: OptionsHandle
    {
        public FlushOptions()
        {
            Native.Instance.rocksdb_flushoptions_create();
        }

        public FlushOptions SetWaitForFlush(bool waitForFlush)
        {
            Native.Instance.rocksdb_flushoptions_set_wait(Handle, waitForFlush);
            return this;
        }
    }
}
