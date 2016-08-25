using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RocksDbSharp
{
    /*
    These wrappers provide translation from the error output of the C API into exceptions
    */
    public abstract partial class Native
    {
        public IntPtr rocksdb_open(
            /* const rocksdb_options_t* */ IntPtr options,
            string name)
        {
            IntPtr errptr;
            var result = rocksdb_open(options, name, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public IntPtr rocksdb_open_for_read_only(
            /* const rocksdb_options_t* */ IntPtr options,
            string name,
            bool error_if_log_file_exists = false)
        {
            IntPtr errptr;
            var result = rocksdb_open_for_read_only(options, name, error_if_log_file_exists, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public IntPtr rocksdb_list_column_families(
            /* const rocksdb_options_t* */ IntPtr options,
            string name,
            out ulong lencf
            )
        {
            IntPtr errptr;
            var result = rocksdb_list_column_families(options, name, out lencf, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public void rocksdb_put(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            string key,
            string val,
            ColumnFamilyHandle cf = null,
            Encoding encoding = null)
        {
            IntPtr errptr;
            rocksdb_put(db, writeOptions, key, val, out errptr, cf, encoding);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_put(
            IntPtr db,
            IntPtr writeOptions,
            byte[] key,
            long keyLength,
            byte[] value,
            long valueLength,
            ColumnFamilyHandle cf)
        {
            IntPtr errptr;
            if (cf == null)
                rocksdb_put(db, writeOptions, key, keyLength, value, valueLength, out errptr);
            else
                rocksdb_put_cf(db, writeOptions, cf.Handle, key, keyLength, value, valueLength, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }


        public string rocksdb_get(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_readoptions_t**/ IntPtr read_options,
            string key,
            ColumnFamilyHandle cf,
            Encoding encoding = null)
        {
            IntPtr errptr;
            var result = rocksdb_get(db, read_options, key, out errptr, cf, encoding);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public IntPtr rocksdb_get(
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            long keyLength,
            out long vallen,
            ColumnFamilyHandle cf)
        {
            IntPtr errptr;
            var result = cf == null
                ? rocksdb_get(db, read_options, key, keyLength, out vallen, out errptr)
                : rocksdb_get_cf(db, read_options, cf.Handle, key, keyLength, out vallen, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public byte[] rocksdb_get(
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            long keyLength,
            ColumnFamilyHandle cf)
        {
            IntPtr errptr;
            var result = rocksdb_get(db, read_options, key, keyLength, out errptr, cf);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public void rocksdb_delete(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            /*const*/ string key,
            ColumnFamilyHandle cf)
        {
            IntPtr errptr;
            rocksdb_delete(db, writeOptions, key, out errptr, cf);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_delete(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            /*const*/ byte[] key,
            long keylen)
        {
            IntPtr errptr;
            rocksdb_delete(db, writeOptions, key, keylen, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_write(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch)
        {
            IntPtr errptr;
            rocksdb_write(db, writeOptions, writeBatch, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public byte[] rocksdb_iter_key(IntPtr iterator)
        {
            ulong length;
            IntPtr buffer = rocksdb_iter_key(iterator, out length);
            byte[] result = new byte[(int)length];
            Marshal.Copy(buffer, result, 0, (int)length);
            // Do not free, this is owned by the iterator and will be freed there
            //rocksdb_free(buffer);
            return result;
        }

        public byte[] rocksdb_iter_value(IntPtr iterator)
        {
            ulong length;
            IntPtr buffer = rocksdb_iter_value(iterator, out length);
            byte[] result = new byte[(int)length];
            Marshal.Copy(buffer, result, 0, (int)length);
            // Do not free, this is owned by the iterator and will be freed there
            //rocksdb_free(buffer);
            return result;
        }

        public IntPtr rocksdb_create_column_family(
            /*rocksdb_t**/ IntPtr db,
            /* const rocksdb_options_t* */ IntPtr column_family_options,
            string column_family_name)
        {
            IntPtr errptr;
            var result = rocksdb_create_column_family(db, column_family_options, column_family_name, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public void rocksdb_drop_column_family(
            /*rocksdb_t**/ IntPtr db,
            /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family_handle
            )
        {
            IntPtr errptr;
            rocksdb_drop_column_family(db, column_family_handle, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

    }
}
