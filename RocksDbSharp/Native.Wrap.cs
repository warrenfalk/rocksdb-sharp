﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable IDE1006 // Intentionally violating naming conventions because this is meant to match the C API
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
            var result = rocksdb_open(options, name, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public IntPtr rocksdb_open_for_read_only(
            /* const rocksdb_options_t* */ IntPtr options,
            string name,
            bool error_if_log_file_exists = false)
        {
            var result = rocksdb_open_for_read_only(options, name, error_if_log_file_exists, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public IntPtr rocksdb_open_column_families(
            /* const rocksdb_options_t* */ IntPtr options,
            string name,
            int num_column_families,
            string[] column_family_names,
            IntPtr[] column_family_options,
            IntPtr[] column_family_handles)
        {
            var result = rocksdb_open_column_families(options, name, num_column_families, column_family_names, column_family_options, column_family_handles, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public IntPtr rocksdb_open_for_read_only_column_families(
            /* const rocksdb_options_t* */ IntPtr options,
            string name,
            int num_column_families,
            string[] column_family_names,
            IntPtr[] column_family_options,
            IntPtr[] column_family_handles,
            bool error_if_log_file_exists)
        {
            var result = rocksdb_open_for_read_only_column_families(options, name, num_column_families, column_family_names, column_family_options, column_family_handles, error_if_log_file_exists, out IntPtr errptr);
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
            var result = rocksdb_list_column_families(options, name, out lencf, out IntPtr errptr);
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
            rocksdb_put(db, writeOptions, key, val, out IntPtr errptr, cf, encoding);
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

        public void rocksdb_merge(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            string key,
            string val,
            ColumnFamilyHandle cf = null,
            Encoding encoding = null)
        {
            rocksdb_merge(db, writeOptions, key, val, out IntPtr errptr, cf, encoding);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_merge(
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
                rocksdb_merge(db, writeOptions, key, keyLength, value, valueLength, out errptr);
            else
                rocksdb_merge_cf(db, writeOptions, cf.Handle, key, keyLength, value, valueLength, out errptr);
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
            var result = rocksdb_get(db, read_options, key, out IntPtr errptr, cf, encoding);
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
            var result = cf == null
                ? rocksdb_get(db, read_options, key, keyLength, out vallen, out IntPtr errptr)
                : rocksdb_get_cf(db, read_options, cf.Handle, key, keyLength, out vallen, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public byte[] rocksdb_get(
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            long keyLength = 0,
            ColumnFamilyHandle cf = null)
        {
            var result = rocksdb_get(db, read_options, key, keyLength == 0 ? key.Length : keyLength, out IntPtr errptr, cf);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public KeyValuePair<string, string>[] rocksdb_multi_get(
            IntPtr db,
            IntPtr read_options,
            string[] keys,
            ColumnFamilyHandle[] cf = null,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            IntPtr[] errptrs = new IntPtr[keys.Length];
            var result = rocksdb_multi_get(db, read_options, keys, cf: cf, errptrs: errptrs, encoding: encoding);
            foreach (var errptr in errptrs)
                if (errptr != IntPtr.Zero)
                    throw new RocksDbException(errptr);
            return result;
        }


        public KeyValuePair<byte[], byte[]>[] rocksdb_multi_get(
            IntPtr db,
            IntPtr read_options,
            byte[][] keys,
            ulong[] keyLengths = null,
            ColumnFamilyHandle[] cf = null)
        {
            IntPtr[] errptrs = new IntPtr[keys.Length];
            var result = rocksdb_multi_get(db, read_options, keys, keyLengths: keyLengths, cf: cf, errptrs: errptrs);
            foreach (var errptr in errptrs)
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
            rocksdb_delete(db, writeOptions, key, out IntPtr errptr, cf);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_delete(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            /*const*/ byte[] key,
            long keylen)
        {
            rocksdb_delete(db, writeOptions, key, keylen, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_write(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch)
        {
            rocksdb_write(db, writeOptions, writeBatch, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_write_writebatch_wi(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            /*(rocksdb_writebatch_wi_t*)*/ IntPtr writeBatchWithIndex)
        {
            rocksdb_write_writebatch_wi(db, writeOptions, writeBatchWithIndex, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public byte[] rocksdb_iter_key(IntPtr iterator)
        {
            IntPtr buffer = rocksdb_iter_key(iterator, out ulong length);
            byte[] result = new byte[(int)length];
            Marshal.Copy(buffer, result, 0, (int)length);
            // Do not free, this is owned by the iterator and will be freed there
            //rocksdb_free(buffer);
            return result;
        }

        public byte[] rocksdb_iter_value(IntPtr iterator)
        {
            IntPtr buffer = rocksdb_iter_value(iterator, out ulong length);
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
            var result = rocksdb_create_column_family(db, column_family_options, column_family_name, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public void rocksdb_drop_column_family(
            /*rocksdb_t**/ IntPtr db,
            /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family_handle
            )
        {
            rocksdb_drop_column_family(db, column_family_handle, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_set_options(IntPtr db, int count, string[] keys, string[] values)
        {
            rocksdb_set_options(db, keys.Length, keys, values, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_sstfilewriter_open(IntPtr writer, string name)
        {
            rocksdb_sstfilewriter_open(writer, name, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_sstfilewriter_finish(IntPtr writer)
        {
            rocksdb_sstfilewriter_finish(writer, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_writebatch_rollback_to_save_point(IntPtr writeBatch)
        {
            rocksdb_writebatch_rollback_to_save_point(writeBatch, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_writebatch_wi_rollback_to_save_point(IntPtr writeBatch)
        {
            rocksdb_writebatch_wi_rollback_to_save_point(writeBatch, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_ingest_external_file(IntPtr db, string[] file_list, ulong list_len, IntPtr opt)
        {
            rocksdb_ingest_external_file(db, file_list, list_len, opt, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public void rocksdb_ingest_external_file_cf(IntPtr db, IntPtr handle, string[] file_list, ulong list_len, IntPtr opt)
        {
            rocksdb_ingest_external_file_cf(db, handle, file_list, list_len, opt, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public unsafe void rocksdb_sstfilewriter_add(
            IntPtr writer,
            byte* key,
            ulong keylen,
            byte* val,
            ulong vallen)
        {
            rocksdb_sstfilewriter_add(writer, key, keylen, val, vallen, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public unsafe void rocksdb_sstfilewriter_add(
            IntPtr writer,
            byte[] key,
            ulong keylen,
            byte[] val,
            ulong vallen)
        {
            rocksdb_sstfilewriter_add(writer, key, keylen, val, vallen, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public unsafe void rocksdb_sstfilewriter_add(
            IntPtr writer,
            string key,
            ulong keylen,
            string val,
            ulong vallen)
        {
            rocksdb_sstfilewriter_add(writer, key, keylen, val, vallen, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
        }

        public string rocksdb_writebatch_wi_get_from_batch(
            IntPtr wb,
            IntPtr options,
            string key,
            ColumnFamilyHandle cf,
            Encoding encoding = null)
        {
            var result = rocksdb_writebatch_wi_get_from_batch(wb, options, key, out IntPtr errptr, cf, encoding);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public IntPtr rocksdb_writebatch_wi_get_from_batch(
            IntPtr wb,
            IntPtr options,
            byte[] key,
            ulong keyLength,
            out ulong vallen,
            ColumnFamilyHandle cf)
        {
            var result = cf == null
                ? rocksdb_writebatch_wi_get_from_batch(wb, options, key, keyLength, out vallen, out IntPtr errptr)
                : rocksdb_writebatch_wi_get_from_batch_cf(wb, options, cf.Handle, key, keyLength, out vallen, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public byte[] rocksdb_writebatch_wi_get_from_batch(
            IntPtr wb,
            IntPtr options,
            byte[] key,
            ulong keyLength = 0,
            ColumnFamilyHandle cf = null)
        {
            var result = rocksdb_writebatch_wi_get_from_batch(wb, options, key, keyLength == 0 ? (ulong)key.Length : keyLength, out IntPtr errptr, cf);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public string rocksdb_writebatch_wi_get_from_batch_and_db(
            IntPtr wb,
            IntPtr db,
            IntPtr read_options,
            string key,
            ColumnFamilyHandle cf,
            Encoding encoding = null)
        {
            var result = rocksdb_writebatch_wi_get_from_batch_and_db(wb, db, read_options, key, out IntPtr errptr, cf, encoding);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public IntPtr rocksdb_writebatch_wi_get_from_batch_and_db(
            IntPtr wb,
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            ulong keyLength,
            out ulong vallen,
            ColumnFamilyHandle cf)
        {
            var result = cf == null
                ? rocksdb_writebatch_wi_get_from_batch_and_db(wb, db, read_options, key, keyLength, out vallen, out IntPtr errptr)
                : rocksdb_writebatch_wi_get_from_batch_and_db_cf(wb, db, read_options, cf.Handle, key, keyLength, out vallen, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }

        public byte[] rocksdb_writebatch_wi_get_from_batch_and_db(
            IntPtr wb,
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            ulong keyLength = 0,
            ColumnFamilyHandle cf = null)
        {
            var result = rocksdb_writebatch_wi_get_from_batch_and_db(wb, db, read_options, key, keyLength == 0 ? (ulong)key.Length : keyLength, out IntPtr errptr, cf);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            return result;
        }
    }
}
