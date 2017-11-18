/*
    The functions in this file provide some wrappers around the lowest level C API to aid in marshalling.
    This is kept separate so that the lowest level imports can be kept as close as possible to c.h from rocksdb.
    See Native.Raw.cs for more information.
*/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Transitional;

#pragma warning disable IDE1006 // Intentionally violating naming conventions because this is meant to match the C API
namespace RocksDbSharp
{
    public abstract partial class Native
    {
        private unsafe string MarshalNullTermAsciiStr(IntPtr nullTermStr)
        {
            byte* bv = (byte*)nullTermStr.ToPointer();
            byte* n = bv;
            while (*n != 0) n++;
            var vlength = n - bv;
            fixed (char* v = new char[vlength])
            {
                Encoding.ASCII.GetChars(bv, (int)vlength, v, (int)vlength);
                Native.Instance.rocksdb_free(nullTermStr);
                return new string(v, 0, (int)vlength);
            }
        }

        public string[] rocksdb_list_column_families(
            /* const rocksdb_options_t* */ IntPtr options,
            string name
            )
        {
            var result = rocksdb_list_column_families(options, name, out ulong lencf, out IntPtr errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            IntPtr[] ptrs = new IntPtr[lencf];
            Marshal.Copy(result, ptrs, 0, (int)lencf);
            string[] strings = new string[lencf];
            for (ulong i = 0; i < lencf; i++)
                strings[i] = Marshal.PtrToStringAnsi(ptrs[i]);
            rocksdb_list_column_families_destroy(result, lencf);
            return strings;
        }

        public void rocksdb_put(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            string key,
            string val,
            out IntPtr errptr,
            ColumnFamilyHandle cf = null,
            Encoding encoding = null)
        {
            unsafe
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;
                fixed (char* k = key, v = val)
                {
                    int klength = key.Length;
                    int vlength = val.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    int bvlength = encoding.GetByteCount(v, vlength);
                    var buffer = Marshal.AllocHGlobal(bklength + bvlength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);
                    byte* bv = bk + bklength;
                    encoding.GetBytes(v, vlength, bv, bvlength);

                    if (cf == null)
                        rocksdb_put(db, writeOptions, bk, (ulong)bklength, bv, (ulong)bvlength, out errptr);
                    else
                        rocksdb_put_cf(db, writeOptions, cf.Handle, bk, (ulong)bklength, bv, (ulong)bvlength, out errptr);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        public string rocksdb_get(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_readoptions_t**/ IntPtr read_options,
            string key,
            out IntPtr errptr,
            ColumnFamilyHandle cf = null,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            unsafe
            {
                fixed (char* k = key)
                {
                    int klength = key.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    var buffer = Marshal.AllocHGlobal(bklength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);

                    var resultPtr = cf == null
                        ? rocksdb_get(db, read_options, bk, bklength, out long bvlength, out errptr)
                        : rocksdb_get_cf(db, read_options, cf.Handle, bk, bklength, out bvlength, out errptr);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);

                    if (errptr != IntPtr.Zero)
                        return null;
                    if (resultPtr == IntPtr.Zero)
                        return null;

                    return MarshalAndFreeRocksDbString(resultPtr, bvlength, encoding);
                }
            }
        }

        private unsafe string MarshalAndFreeRocksDbString(IntPtr resultPtr, long resultLength, Encoding encoding)
        {
            var result = CurrentFramework.CreateString((sbyte*)resultPtr.ToPointer(), 0, (int)resultLength, encoding);
            rocksdb_free(resultPtr);
            return result;
        }
        private unsafe string MarshalString(IntPtr resultPtr, long resultLength, Encoding encoding)
        {
            return CurrentFramework.CreateString((sbyte*)resultPtr.ToPointer(), 0, (int)resultLength, encoding);
        }

        public byte[] rocksdb_get(
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            long keyLength,
            out IntPtr errptr,
            ColumnFamilyHandle cf = null)
        {
            var resultPtr = cf == null
                ? rocksdb_get(db, read_options, key, keyLength, out long valueLength, out errptr)
                : rocksdb_get_cf(db, read_options, cf.Handle, key, keyLength, out valueLength, out errptr);
            if (errptr != IntPtr.Zero)
                return null;
            if (resultPtr == IntPtr.Zero)
                return null;
            var result = new byte[valueLength];
            Marshal.Copy(resultPtr, result, 0, (int)valueLength);
            rocksdb_free(resultPtr);
            return result;
        }

        /// <summary>
        /// Executes a multi_get with automatic marshalling
        /// </summary>
        /// <param name="db"></param>
        /// <param name="read_options"></param>
        /// <param name="keys"></param>
        /// <param name="numKeys">when non-zero, specifies the number of keys in the array to fetch</param>
        /// <param name="keyLengths">when non-null specifies the lengths of each key to fetch</param>
        /// <param name="errptrs">when non-null, must be an array that will be populated with error codes</param>
        /// <param name="values">when non-null is a pre-allocated array to put the resulting values in</param>
        /// <param name="cf"></param>
        /// <returns></returns>
        public unsafe KeyValuePair<byte[], byte[]>[] rocksdb_multi_get(
            IntPtr db,
            IntPtr read_options,
            byte[][] keys,
            IntPtr[] errptrs,
            ulong numKeys = 0,
            ulong[] keyLengths = null,
            KeyValuePair<byte[], byte[]>[] values = null,
            ColumnFamilyHandle[] cf = null)
        {
            int count = numKeys == 0 ? keys.Length : (int)numKeys;
            GCHandle[] pinned = new GCHandle[count];
            IntPtr[] keyPtrs = new IntPtr[count];
            IntPtr[] valuePtrs = new IntPtr[count];
            ulong[] valueLengths = new ulong[count];

            if (values == null)
                values = new KeyValuePair<byte[], byte[]>[count];
            if (errptrs == null)
                errptrs = new IntPtr[count];
            if (keyLengths == null)
            {
                keyLengths = new ulong[count];
                for (int i = 0; i < count; i++)
                    keyLengths[i] = (ulong)keys[i].Length;
            }

            // first we have to pin and take the address of each key
            for (int i = 0; i < count; i++)
            {
                var gch = GCHandle.Alloc(keys[i], GCHandleType.Pinned);
                pinned[i] = gch;
                keyPtrs[i] = gch.AddrOfPinnedObject();
            }
            if (cf == null)
            {
                rocksdb_multi_get(db, read_options, (ulong)count, keyPtrs, keyLengths, valuePtrs, valueLengths, errptrs);
            }
            else
            {
                IntPtr[] cfhs = new IntPtr[cf.Length];
                for (int i = 0; i < count; i++)
                    cfhs[i] = cf[i].Handle;
                rocksdb_multi_get_cf(db, read_options, cfhs, (ulong)count, keyPtrs, keyLengths, valuePtrs, valueLengths, errptrs);
            }
            // unpin the keys
            foreach (var gch in pinned)
                gch.Free();

            // now marshal all of the values
            for (int i = 0; i < count; i++)
            {
                var valuePtr = valuePtrs[i];
                if (valuePtr != IntPtr.Zero)
                {
                    var valueLength = valueLengths[i];
                    byte[] value = new byte[valueLength];
                    Marshal.Copy(valuePtr, value, 0, (int)valueLength);
                    values[i] = new KeyValuePair<byte[], byte[]>(keys[i], value);
                    rocksdb_free(valuePtr);
                }
                else
                {
                    values[i] = new KeyValuePair<byte[], byte[]>(keys[i], null);
                }
            }
            return values;
        }
        /// <summary>
        /// Executes a multi_get with automatic marshalling
        /// </summary>
        /// <param name="db"></param>
        /// <param name="read_options"></param>
        /// <param name="keys"></param>
        /// <param name="numKeys">when non-zero, specifies the number of keys in the array to fetch</param>
        /// <param name="keyLengths">when non-null specifies the lengths of each key to fetch</param>
        /// <param name="errptrs">when non-null, must be an array that will be populated with error codes</param>
        /// <param name="values">when non-null is a pre-allocated array to put the resulting values in</param>
        /// <param name="cf"></param>
        /// <returns></returns>
        public unsafe KeyValuePair<string, string>[] rocksdb_multi_get(
            IntPtr db,
            IntPtr read_options,
            string[] keys,
            IntPtr[] errptrs,
            ulong numKeys = 0,
            KeyValuePair<string, string>[] values = null,
            ColumnFamilyHandle[] cf = null,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            int count = numKeys == 0 ? keys.Length : (int)numKeys;
            IntPtr[] keyPtrs = new IntPtr[count];
            var keyLengths = new ulong[count];
            IntPtr[] valuePtrs = new IntPtr[count];
            ulong[] valueLengths = new ulong[count];

            if (values == null)
                values = new KeyValuePair<string, string>[count];
            if (errptrs == null)
                errptrs = new IntPtr[count];

            // first we have to encode each key
            for (int i = 0; i < count; i++)
            {
                var key = keys[i];
                fixed (char* k = key)
                {
                    var klength = key.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    var bk = Marshal.AllocHGlobal(bklength);
                    encoding.GetBytes(k, klength, (byte*)bk.ToPointer(), bklength);
                    keyPtrs[i] = bk;
                    keyLengths[i] = (ulong)bklength;
                }
            }
            if (cf == null)
            {
                rocksdb_multi_get(db, read_options, (ulong)count, keyPtrs, keyLengths, valuePtrs, valueLengths, errptrs);
            }
            else
            {
                IntPtr[] cfhs = new IntPtr[cf.Length];
                for (int i = 0; i < count; i++)
                    cfhs[i] = cf[i].Handle;
                rocksdb_multi_get_cf(db, read_options, cfhs, (ulong)count, keyPtrs, keyLengths, valuePtrs, valueLengths, errptrs);
            }
            // free the buffers allocated for each encoded key
            foreach (var keyPtr in keyPtrs)
                Marshal.FreeHGlobal(keyPtr);

            // now marshal all of the values
            for (int i = 0; i < count; i++)
            {
                var resultPtr = valuePtrs[i];
                if (resultPtr != IntPtr.Zero)
                {
                    var bv = (sbyte*)resultPtr.ToPointer();
                    var bvLength = valueLengths[i];
                    values[i] = new KeyValuePair<string, string>(keys[i], CurrentFramework.CreateString(bv, 0, (int)bvLength, encoding));
                    rocksdb_free(resultPtr);
                }
                else
                {
                    values[i] = new KeyValuePair<string, string>(keys[i], null);
                }
            }
            return values;
        }

        public void rocksdb_delete(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            /*const*/ string key,
            out IntPtr errptr,
            ColumnFamilyHandle cf,
            Encoding encoding = null)
        {
            var bkey = (encoding ?? Encoding.UTF8).GetBytes(key);
            if (cf == null)
                rocksdb_delete(db, writeOptions, bkey, bkey.GetLongLength(0), out errptr);
            else
                rocksdb_delete_cf(db, writeOptions, cf.Handle, bkey, bkey.GetLongLength(0), out errptr);
        }

        public string rocksdb_options_statistics_get_string_marshaled(IntPtr opts)
        {
            return MarshalNullTermAsciiStr(rocksdb_options_statistics_get_string(opts));
        }

        public void rocksdb_writebatch_put(IntPtr writeOptions, string key, string val, Encoding encoding)
        {
            unsafe
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;
                fixed (char* k = key, v = val)
                {
                    int klength = key.Length;
                    int vlength = val.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    int bvlength = encoding.GetByteCount(v, vlength);
                    var buffer = Marshal.AllocHGlobal(bklength + bvlength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);
                    byte* bv = bk + bklength;
                    encoding.GetBytes(v, vlength, bv, bvlength);

                    rocksdb_writebatch_put(writeOptions, bk, (ulong)bklength, bv, (ulong)bvlength);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        public void rocksdb_writebatch_wi_put(IntPtr writeOptions, string key, string val, Encoding encoding)
        {
            unsafe
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;
                fixed (char* k = key, v = val)
                {
                    int klength = key.Length;
                    int vlength = val.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    int bvlength = encoding.GetByteCount(v, vlength);
                    var buffer = Marshal.AllocHGlobal(bklength + bvlength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);
                    byte* bv = bk + bklength;
                    encoding.GetBytes(v, vlength, bv, bvlength);

                    rocksdb_writebatch_wi_put(writeOptions, bk, (ulong)bklength, bv, (ulong)bvlength);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        public void rocksdb_iter_seek(
            IntPtr iter,
            string key,
            Encoding encoding = null)
        {
            unsafe
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;
                fixed (char* k = key)
                {
                    int klength = key.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    var buffer = Marshal.AllocHGlobal(bklength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);

                    rocksdb_iter_seek(iter, bk, (ulong)bklength);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        public void rocksdb_iter_seek_for_prev(
            IntPtr iter,
            string key,
            Encoding encoding = null)
        {
            unsafe
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;
                fixed (char* k = key)
                {
                    int klength = key.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    var buffer = Marshal.AllocHGlobal(bklength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);

                    rocksdb_iter_seek_for_prev(iter, bk, (ulong)bklength);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

#if DEBUG
        // Zero out memory before freeing it when in debug mode so that we see problems
        // resulting from the contents still being used on the native side
        private unsafe static void Zero(byte* bk, int bklength)
        {
            var end = bk + bklength;
            for (; bk < end; bk++)
                *bk = 0;
        }
#endif

        public string rocksdb_iter_key_string(
            /*rocksdb_t**/ IntPtr iter,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            unsafe
            {
                var resultPtr = rocksdb_iter_key(iter, out ulong bklength);

                return MarshalString(resultPtr, (long)bklength, encoding);
            }
        }

        public string rocksdb_iter_value_string(
            /*rocksdb_t**/ IntPtr iter,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            unsafe
            {
                var resultPtr = rocksdb_iter_value(iter, out ulong bvlength);

                return MarshalString(resultPtr, (long)bvlength, encoding);
            }
        }

        public byte[] rocksdb_writebatch_data(IntPtr wbHandle)
        {
            var resultPtr = rocksdb_writebatch_data(wbHandle, out ulong size);
            var data = new byte[size];
            Marshal.Copy(resultPtr, data, 0, (int)size);
            // Do not free this memory because it is owned by the write batch and will be freed when it is disposed
            // rocksdb_free(resultPtr);
            return data;
        }

        public byte[] rocksdb_writebatch_wi_data(IntPtr wbHandle)
        {
            var resultPtr = rocksdb_writebatch_wi_data(wbHandle, out ulong size);
            var data = new byte[size];
            Marshal.Copy(resultPtr, data, 0, (int)size);
            // Do not free this memory because it is owned by the write batch and will be freed when it is disposed
            // rocksdb_free(resultPtr);
            return data;
        }

        public int rocksdb_writebatch_data(IntPtr wbHandle, byte[] buffer, int offset, int length)
        {
            var resultPtr = rocksdb_writebatch_data(wbHandle, out ulong size);
            bool fits = (int)size <= length;
            if (!fits)
            {
                // Do not free this memory because it is owned by the write batch and will be freed when it is disposed
                // rocksdb_free(resultPtr);
                return -1;
            }
            Marshal.Copy(resultPtr, buffer, offset, (int)size);
            // Do not free this memory because it is owned by the write batch and will be freed when it is disposed
            // rocksdb_free(resultPtr);
            return (int)size;
        }

        public int rocksdb_writebatch_wi_data(IntPtr wbHandle, byte[] buffer, int offset, int length)
        {
            var resultPtr = rocksdb_writebatch_wi_data(wbHandle, out ulong size);
            bool fits = (int)size <= length;
            if (!fits)
            {
                // Do not free this memory because it is owned by the write batch and will be freed when it is disposed
                // rocksdb_free(resultPtr);
                return -1;
            }
            Marshal.Copy(resultPtr, buffer, offset, (int)size);
            // Do not free this memory because it is owned by the write batch and will be freed when it is disposed
            // rocksdb_free(resultPtr);
            return (int)size;
        }

        public string rocksdb_property_value_string(IntPtr db, string propname)
        {
            return MarshalNullTermAsciiStr(rocksdb_property_value(db, propname));
        }

        public string rocksdb_property_value_cf_string(IntPtr db, IntPtr column_family, string propname)
        {
            return MarshalNullTermAsciiStr(rocksdb_property_value_cf(db, column_family, propname));
        }

        public unsafe void rocksdb_sstfilewriter_add(
            IntPtr writer,
            string key,
            ulong keylen,
            string val,
            ulong vallen,
            out IntPtr errptr,
            Encoding encoding = null)
        {
            unsafe
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;
                fixed (char* k = key, v = val)
                {
                    int klength = key.Length;
                    int vlength = val.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    int bvlength = encoding.GetByteCount(v, vlength);
                    var buffer = Marshal.AllocHGlobal(bklength + bvlength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);
                    byte* bv = bk + bklength;
                    encoding.GetBytes(v, vlength, bv, bvlength);

                    rocksdb_sstfilewriter_add(writer, bk, (ulong)bklength, bv, (ulong)bvlength, out errptr);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        public string rocksdb_writebatch_wi_get_from_batch(
            IntPtr wb,
            IntPtr options,
            string key,
            out IntPtr errptr,
            ColumnFamilyHandle cf = null,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            unsafe
            {
                fixed (char* k = key)
                {
                    int klength = key.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    var buffer = Marshal.AllocHGlobal(bklength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);

                    var resultPtr = cf == null
                        ? rocksdb_writebatch_wi_get_from_batch(wb, options, bk, (ulong)bklength, out ulong bvlength, out errptr)
                        : rocksdb_writebatch_wi_get_from_batch_cf(wb, options, cf.Handle, bk, (ulong)bklength, out bvlength, out errptr);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);

                    if (errptr != IntPtr.Zero)
                        return null;
                    if (resultPtr == IntPtr.Zero)
                        return null;

                    return MarshalAndFreeRocksDbString(resultPtr, (long)bvlength, encoding);
                }
            }
        }

        public byte[] rocksdb_writebatch_wi_get_from_batch(
            IntPtr wb,
            IntPtr options,
            byte[] key,
            ulong keyLength,
            out IntPtr errptr,
            ColumnFamilyHandle cf = null)
        {
            var resultPtr = cf == null
                ? rocksdb_writebatch_wi_get_from_batch(wb, options, key, keyLength, out ulong valueLength, out errptr)
                : rocksdb_writebatch_wi_get_from_batch_cf(wb, options, cf.Handle, key, keyLength, out valueLength, out errptr);
            if (errptr != IntPtr.Zero)
                return null;
            if (resultPtr == IntPtr.Zero)
                return null;
            var result = new byte[valueLength];
            Marshal.Copy(resultPtr, result, 0, (int)valueLength);
            rocksdb_free(resultPtr);
            return result;
        }

        public string rocksdb_writebatch_wi_get_from_batch_and_db(
            IntPtr wb,
            IntPtr db,
            IntPtr read_options,
            string key,
            out IntPtr errptr,
            ColumnFamilyHandle cf = null,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            unsafe
            {
                fixed (char* k = key)
                {
                    int klength = key.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    var buffer = Marshal.AllocHGlobal(bklength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);

                    var resultPtr = cf == null
                        ? rocksdb_writebatch_wi_get_from_batch_and_db(wb, db, read_options, bk, (ulong)bklength, out ulong bvlength, out errptr)
                        : rocksdb_writebatch_wi_get_from_batch_and_db_cf(wb, db, read_options, cf.Handle, bk, (ulong)bklength, out bvlength, out errptr);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);

                    if (errptr != IntPtr.Zero)
                        return null;
                    if (resultPtr == IntPtr.Zero)
                        return null;

                    return MarshalAndFreeRocksDbString(resultPtr, (long)bvlength, encoding);
                }
            }
        }

        public byte[] rocksdb_writebatch_wi_get_from_batch_and_db(
            IntPtr wb,
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            ulong keyLength,
            out IntPtr errptr,
            ColumnFamilyHandle cf = null)
        {
            var resultPtr = cf == null
                ? rocksdb_writebatch_wi_get_from_batch_and_db(wb, db, read_options, key, keyLength, out ulong valueLength, out errptr)
                : rocksdb_writebatch_wi_get_from_batch_and_db_cf(wb, db, read_options, cf.Handle, key, keyLength, out valueLength, out errptr);
            if (errptr != IntPtr.Zero)
                return null;
            if (resultPtr == IntPtr.Zero)
                return null;
            var result = new byte[valueLength];
            Marshal.Copy(resultPtr, result, 0, (int)valueLength);
            rocksdb_free(resultPtr);
            return result;
        }

    }
}
