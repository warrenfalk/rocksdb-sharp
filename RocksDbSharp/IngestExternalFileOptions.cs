using System;
using System.Collections.Generic;
using System.Text;

namespace RocksDbSharp
{
    public class IngestExternalFileOptions
    {
        public IntPtr Handle { get; protected set; }

        public IngestExternalFileOptions()
        {
            Handle = Native.Instance.rocksdb_ingestexternalfileoptions_create();
        }

        ~IngestExternalFileOptions()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_ingestexternalfileoptions_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }

        public IngestExternalFileOptions SetMoveFiles(bool moveFiles)
        {
            Native.Instance.rocksdb_ingestexternalfileoptions_set_move_files(Handle, moveFiles);
            return this;
        }

        public IngestExternalFileOptions SetSnapshotConsistency(bool snapshotConsistency)
        {
            Native.Instance.rocksdb_ingestexternalfileoptions_set_snapshot_consistency(Handle, snapshotConsistency);
            return this;
        }

        public IngestExternalFileOptions SetAllowGlobalSeqno(bool allow)
        {
            Native.Instance.rocksdb_ingestexternalfileoptions_set_allow_global_seqno(Handle, allow);
            return this;
        }

        public IngestExternalFileOptions SetAllowBlockingFlush(bool allow)
        {
            Native.Instance.rocksdb_ingestexternalfileoptions_set_allow_blocking_flush(Handle, allow);
            return this;
        }
    }
}
