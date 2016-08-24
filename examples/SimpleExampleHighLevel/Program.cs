using RocksDbSharp;
using System;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace SimpleExampleHighLevel
{
    class Program
    {
        static void Main(string[] args)
        {
            string temp = Path.GetTempPath();
            string path = Environment.ExpandEnvironmentVariables(Path.Combine(temp, "rocksdb_simple_hl_example"));
            // the Options class contains a set of configurable DB options
            // that determines the behavior of a database
            // Why is the syntax, SetXXX(), not very C#-like? See Options for an explanation
            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .EnableStatistics();
            using (var db = RocksDb.Open(options, path))
            {
                try
                {
                    var rand = new Random(100);
                    var rand2 = new Random(100);
                    for (int k = 0; k < 1000000; k++)
                    {
                        for (int i = 0; i < 1000; i++)
                        {
                            var key = new byte[18];
                            var value = new byte[128];
                            rand.NextBytes(key);
                            rand.NextBytes(value);
                            byte[] write;
                            using (var wb = new WriteBatch())
                            {
                                wb.Put(key, value);
                                write = wb.ToBytes();
                            }

                            using (var wb = new WriteBatch(write))
                            {
                                db.Write(wb);
                            }
                        }
                        for (int i = 0; i < 1000; i++)
                        {
                            var key = new byte[18];
                            var value = new byte[128];
                            rand2.NextBytes(key);
                            rand2.NextBytes(value);
                            var value2 = db.Get(key);
                        }
                        Console.WriteLine("{0}k", k);
                        //Console.WriteLine(options.GetStatisticsString());
                        Console.WriteLine(db.GetProperty("rocksdb.cur-size-all-mem-tables"));
                    }
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

                    {
                        // Removal of non-existent keys
                        db.Remove("I don't exist");
                    }

                    {
                        // Write batches
                        // With strings
                        using (WriteBatch batch = new WriteBatch()
                            .Put("one", "uno")
                            .Put("two", "deuce")
                            .Put("two", "dos")
                            .Put("three", "tres"))
                        {
                            db.Write(batch);
                        }

                        // With bytes
                        var utf8 = Encoding.UTF8;
                        using (WriteBatch batch = new WriteBatch()
                            .Put(utf8.GetBytes("four"), new byte[] { 4, 4, 4 } )
                            .Put(utf8.GetBytes("five"), new byte[] { 5, 5, 5 } ))
                        {
                            db.Write(batch);
                        }
                    }

                    var two = db.Get("two");
                    Debug.Assert(two == "dos");

                    {
                        // Iterators
                        using (var iterator = db.NewIterator(
                            readOptions: new ReadOptions()
                                .SetIterateUpperBound("t")
                                ))
                        {
                            iterator.Seek("k");
                            Debug.Assert(iterator.Valid());
                            Debug.Assert(iterator.StringKey() == "key");
                            iterator.Next();
                            Debug.Assert(iterator.Valid());
                            Debug.Assert(iterator.StringKey() == "one");
                            Debug.Assert(iterator.StringValue() == "uno");
                            iterator.Next();
                            Debug.Assert(!iterator.Valid());
                        }
                    }

                }
                catch (RocksDbException)
                {

                }
            }
        }
    }
}
