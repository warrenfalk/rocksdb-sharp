using RocksDbSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RocksDbSharpTest
{
    public class CompactionFilterTest
    {
        [Fact]
        public void TestAllDelegates()
        {
            var dbName = "TestCompactionFilterDb";
            FlushOptions flushOptions = new FlushOptions().SetWaitForFlush(true);
            DbOptions options = new DbOptions().SetCreateIfMissing(true);
            var filter = new CompactionFilter(DefaultNameDelegate, TestFilterDelegate, DefaultDestructorDelegate, IntPtr.Zero);
            options.SetCompactionFilter(filter.Handle);
            options.SetDisableAutoCompactions(1);

            SetupAndPopulate(dbName, flushOptions, options);
        }        

        private static void SetupAndPopulate(string dbName, FlushOptions flushOptions, DbOptions options)
        {
            DeleteDb(dbName);
            using (var db = RocksDb.Open(options, dbName))
            {
                byte[] key = Encoding.UTF8.GetBytes("keep_a");
                byte[] value = Encoding.UTF8.GetBytes("1");

                db.Put(key, value);

                key = Encoding.UTF8.GetBytes("keep_b");
                value = Encoding.UTF8.GetBytes("2");

                db.Put(key, value);

                key = Encoding.UTF8.GetBytes("Remove_c");
                value = Encoding.UTF8.GetBytes("3");

                db.Put(key, value);

                key = Encoding.UTF8.GetBytes("Replace_d");
                value = Encoding.UTF8.GetBytes("4");

                db.Put(key, value);

                db.Flush(flushOptions);
                db.CompactRange(null, null, null);

                key = Encoding.UTF8.GetBytes("keep_a");
                var result = db.Get(key);
                Assert.NotNull(result);

                key = Encoding.UTF8.GetBytes("Remove_c");
                result = db.Get(key);
                Assert.Null(result);

                key = Encoding.UTF8.GetBytes("Replace_d");
                result = db.Get(key);
                Assert.NotNull(result);
                Assert.Equal(result, Encoding.UTF8.GetBytes("8"));
            }
        }

        public char TestFilterDelegate(IntPtr p0, int level, IntPtr key, UIntPtr key_length, IntPtr existing_value, UIntPtr value_length, IntPtr new_value, IntPtr new_value_length, IntPtr value_changed)
        {
            int keyLength = (int)key_length.ToUInt64();
            byte[] keyValue = new byte[keyLength];
            Marshal.Copy(key, keyValue, 0, keyLength);

            int valueLength = (int)value_length.ToUInt64();
            byte[] existingValue = new byte[valueLength];
            Marshal.Copy(existing_value, existingValue, 0, valueLength);

            var keyStr = Encoding.UTF8.GetString(keyValue);
            if (keyStr.StartsWith("Remove"))
            {
                return (char)1;
            }

            if (keyStr.StartsWith("Replace"))
            {
                var valueStr = Encoding.UTF8.GetString(existingValue);
                int val = Convert.ToInt32(valueStr);
                byte[] newValue = Encoding.UTF8.GetBytes($"{val * 2}");

                if (newValue != null && newValue.Length != 0)
                {
                    IntPtr buffer = Marshal.AllocHGlobal(newValue.Length);
                    try
                    {
                        Marshal.Copy(newValue, 0, buffer, newValue.Length);
                        Marshal.WriteIntPtr(new_value, buffer);
                        Marshal.WriteByte(value_changed, 1);

                        var intSize = IntPtr.Size;
                        switch (intSize)
                        {
                            case 4:
                                Marshal.WriteInt32(new_value_length, newValue.Length);
                                break;
                            case 8:
                                Marshal.WriteInt64(new_value_length, newValue.Length);
                                break;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
            }

            return (char)0;
        }

        public unsafe void DefaultDestructorDelegate(IntPtr state)
        {
            Marshal.FreeHGlobal(state);
        }

        public unsafe IntPtr DefaultNameDelegate(IntPtr state)
        {
            return Marshal.StringToHGlobalAnsi("DefaultCompactionFilter");
        }

        public static void DeleteDb(string dbName)
        {
            if (Directory.Exists(dbName))
            {
                Directory.Delete(dbName, true);
            }
        }
    }
}
