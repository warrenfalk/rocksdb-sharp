// Native class for custom additions missing in rocksdb package

using char_ptr_ptr = System.IntPtr;
using const_size_t = System.UIntPtr;
using rocksdb_t_ptr = System.IntPtr;
using System;

namespace RocksDbSharp
{
    public partial class Native
    {
        public abstract void rocksdb_compact_file(
             rocksdb_t_ptr db,
             string[] file_list,
             const_size_t list_len,
             int output_level, out char_ptr_ptr errptr);

        public void rocksdb_compact_file(
             rocksdb_t_ptr db,
             string[] file_list,
             const_size_t list_len,
             int output_level)
        {
            rocksdb_compact_file(db, file_list, list_len, output_level, out char_ptr_ptr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }
    }
}