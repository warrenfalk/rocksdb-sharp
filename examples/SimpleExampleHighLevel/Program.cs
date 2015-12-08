using RocksDbSharp;
using System;
using System.Text;

namespace SimpleExampleHighLevel
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Environment.ExpandEnvironmentVariables("%TMP%/rocksdb_simple_hl_example");
            // the Options class contains a set of configurable DB options
            // that determines the behavior of a database
            // Why is the syntax, SetXXX(), not very C#-like? See Options for an explanation
            using (var options = new Options().SetCreateIfMissing(true))
            using (var db = RocksDb.Open(options, path))
            {
                try
                {
                    {
                        // With strings
                        string value = db.Get("key");
                        db.Put("key", "value");
                        value = db.Get("key");
                        string iWillBeNull = db.Get("non-existent-key");
                        db.Remove("key");
                    }

                    {
                        // With bytes
                        var key = Encoding.UTF8.GetBytes("key");
                        byte[] value = Encoding.UTF8.GetBytes("value");
                        db.Put(key, value);
                        value = db.Get(key);
                        byte[] iWillBeNull = db.Get(new byte[] { 0, 1, 2 });
                        db.Remove(key);

                        db.Put(key, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                    }

                    {
                        // With buffers
                        var key = Encoding.UTF8.GetBytes("key");
                        var buffer = new byte[100];
                        long length = db.Get(key, buffer, 0, buffer.Length);
                    }
                }
                catch (RocksDbException)
                {

                }
            }
        }
    }
}
