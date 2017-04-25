using RocksDbSharp;
using System;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace PrefixExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string temp = Path.GetTempPath();
            string path = Environment.ExpandEnvironmentVariables(Path.Combine(temp, "rocksdb_prefix_example"));
            var bbto = new BlockBasedTableOptions()
                .SetFilterPolicy(BloomFilterPolicy.Create(10, false))
                .SetWholeKeyFiltering(false)
                ;
            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .SetCreateMissingColumnFamilies(true)
                ;
            var columnFamilies = new ColumnFamilies
            {
                { "default", new ColumnFamilyOptions().OptimizeForPointLookup(256) },
                { "test", new ColumnFamilyOptions()
                    //.SetWriteBufferSize(writeBufferSize)
                    //.SetMaxWriteBufferNumber(maxWriteBufferNumber)
                    //.SetMinWriteBufferNumberToMerge(minWriteBufferNumberToMerge)
                    .SetMemtableHugePageSize(2 * 1024 * 1024)
                    .SetPrefixExtractor(SliceTransform.CreateFixedPrefix((ulong)8))
                    .SetBlockBasedTableFactory(bbto)
                },
            };
            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                var cf = db.GetColumnFamily("test");

                db.Put("00000000Zero", "", cf: cf);
                db.Put("00000000One", "", cf: cf);
                db.Put("00000000Two", "", cf: cf);
                db.Put("00000000Three", "", cf: cf);
                db.Put("00000001Red", "", cf: cf);
                db.Put("00000001Green", "", cf: cf);
                db.Put("00000001Black", "", cf: cf);
                db.Put("00000002Apple", "", cf: cf);
                db.Put("00000002Cranberry", "", cf: cf);
                db.Put("00000002Banana", "", cf: cf);

                var readOptions = new ReadOptions();
                using (var iter = db.NewIterator(readOptions: readOptions, cf: cf))
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    var b = Encoding.UTF8.GetBytes("00000001");
                    iter.Seek(b);
                    while (iter.Valid())
                    {
                        Console.WriteLine(iter.StringKey());
                        iter.Next();
                    }
                }
            }
            Console.WriteLine("Done...");
        }
    }
}
