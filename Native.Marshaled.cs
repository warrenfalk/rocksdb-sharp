/*
    The functions in this file provide some wrappers around the lowest level C API to aid in marshalling.
    This is kept separate so that the lowest level imports can be kept as close as possible to c.h from rocksdb.
    See Native.Raw.cs for more information.
*/
using System;
using System.Runtime.InteropServices;
using System.Text;

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
            IntPtr errptr;
            ulong lencf;
            var result = rocksdb_list_column_families(options, name, out lencf, out errptr);
            if (errptr != IntPtr.Zero)
                throw new RocksDbException(errptr);
            IntPtr[] ptrs = new IntPtr[lencf];
            Marshal.Copy(result, ptrs, 0, (int)lencf);
            rocksdb_free(result);
            string[] strings = new string[lencf];
            for (ulong i = 0; i < lencf; i++)
                strings[i] = Marshal.PtrToStringAnsi(ptrs[i]);
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
            long bvlength;
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
                        ? rocksdb_get(db, read_options, bk, bklength, out bvlength, out errptr)
                        : rocksdb_get_cf(db, read_options, cf.Handle, bk, bklength, out bvlength, out errptr);
#if DEBUG
                    Zero(bk, bklength);
#endif
                    Marshal.FreeHGlobal(buffer);

                    if (errptr != IntPtr.Zero)
                        return null;
                    if (resultPtr == IntPtr.Zero)
                        return null;

                    byte* bv = (byte*)resultPtr.ToPointer();
                    int vlength = encoding.GetCharCount(bv, (int)bvlength);
                    fixed (char* v = new char[vlength])
                    {
                        encoding.GetChars(bv, (int)bvlength, v, vlength);
                        rocksdb_free(resultPtr);
                        return new string(v, 0, vlength);
                    }
                }
            }
        }

        public byte[] rocksdb_get(
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            long keyLength,
            out IntPtr errptr,
            ColumnFamilyHandle cf)
        {
            long valueLength;
            var resultPtr = cf == null
                ? rocksdb_get(db, read_options, key, keyLength, out valueLength, out errptr)
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
                rocksdb_delete(db, writeOptions, bkey, bkey.LongLength, out errptr);
            else
                rocksdb_delete_cf(db, writeOptions, cf.Handle, bkey, bkey.LongLength, out errptr);
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
            ulong bklength;
            unsafe
            {
                var resultPtr = rocksdb_iter_key(iter, out bklength);

                byte* bk = (byte*)resultPtr.ToPointer();
                int klength = encoding.GetCharCount(bk, (int)bklength);
                fixed (char* k = new char[klength])
                {
                    encoding.GetChars(bk, (int)bklength, k, klength);
                    //rocksdb_free(resultPtr);
                    return new string(k, 0, klength);
                }
            }
        }

        public string rocksdb_iter_value_string(
            /*rocksdb_t**/ IntPtr iter,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            ulong bvlength;
            unsafe
            {
                var resultPtr = rocksdb_iter_value(iter, out bvlength);

                byte* bv = (byte*)resultPtr.ToPointer();
                int vlength = encoding.GetCharCount(bv, (int)bvlength);
                fixed (char* v = new char[vlength])
                {
                    encoding.GetChars(bv, (int)bvlength, v, vlength);
                    //rocksdb_free(resultPtr);
                    return new string(v, 0, vlength);
                }
            }
        }

        public byte[] rocksdb_writebatch_data(IntPtr wbHandle)
        {
            ulong size;
            var resultPtr = rocksdb_writebatch_data(wbHandle, out size);
            var data = new byte[size];
            Marshal.Copy(resultPtr, data, 0, (int)size);
            // Do not free this memory because it is owned by the write batch and will be freed when it is disposed
            // rocksdb_free(resultPtr);
            return data;
        }

        public int rocksdb_writebatch_data(IntPtr wbHandle, byte[] buffer, int offset, int length)
        {
            ulong size;
            var resultPtr = rocksdb_writebatch_data(wbHandle, out size);
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

    }
}
