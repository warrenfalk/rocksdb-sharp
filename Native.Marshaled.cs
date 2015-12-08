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
    public static partial class Native
    {
        public static void rocksdb_put(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            string key,
            string val,
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

                    rocksdb_put(db, writeOptions, bk, (ulong)bklength, bv, (ulong)bvlength, out errptr);
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }

        public static string rocksdb_get(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_readoptions_t**/ IntPtr read_options,
            string key,
            out IntPtr errptr,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            ulong bvlength;
            unsafe
            {
                fixed (char *k = key)
                {
                    int klength = key.Length;
                    int bklength = encoding.GetByteCount(k, klength);
                    var buffer = Marshal.AllocHGlobal(bklength);
                    byte* bk = (byte*)buffer.ToPointer();
                    encoding.GetBytes(k, klength, bk, bklength);

                    var resultPtr = rocksdb_get(db, read_options, bk, (ulong)bklength, out bvlength, out errptr);
                    Marshal.FreeHGlobal(buffer);

                    if (errptr != IntPtr.Zero)
                        return null;
                    if (resultPtr == IntPtr.Zero)
                        return null;

                    byte* bv = (byte*)resultPtr.ToPointer();
                    char[] vchars;
                    int vlength = encoding.GetCharCount(bv, (int)bvlength);
                    fixed (char *v = vchars = new char[vlength])
                    {
                        encoding.GetChars(bv, (int)bvlength, v, vlength);
                        return new string(v, 0, vlength);
                    }
                }
            }
        }

        public static byte[] rocksdb_get(
            IntPtr db,
            IntPtr read_options,
            byte[] key,
            long keyLength,
            out IntPtr errptr)
        {
            long valueLength;
            var resultPtr = rocksdb_get(db, read_options, key, keyLength, out valueLength, out errptr);
            if (errptr != IntPtr.Zero)
                return null;
            if (resultPtr == IntPtr.Zero)
                return null;
            var result = new byte[valueLength];
            Marshal.Copy(resultPtr, result, 0, (int)valueLength);
            return result;
        }



        public static void rocksdb_delete(
            /*rocksdb_t**/ IntPtr db,
            /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
            /*const*/ string key,
            out IntPtr errptr,
            Encoding encoding = null)
        {
            var bkey = (encoding ?? Encoding.UTF8).GetBytes(key);
            rocksdb_delete(db, writeOptions, bkey, bkey.LongLength, out errptr);
        }
    }
}
