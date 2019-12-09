using System;
using Xunit;
using System.IO;
using RocksDbSharp;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RocksDbSharpTest
{
    public class FunctionalTests
    {
        [Fact]
        public void FunctionalTest()
        {
            string temp = Path.GetTempPath();
            var testdir = Path.Combine(temp, "functional_test");
            var testdb = Path.Combine(testdir, "main");
            var testcp = Path.Combine(testdir, "cp");
            var path = Environment.ExpandEnvironmentVariables(testdb);
            var cppath = Environment.ExpandEnvironmentVariables(testcp);

            if (Directory.Exists(testdir))
                Directory.Delete(testdir, true);
            Directory.CreateDirectory(testdir);

            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .EnableStatistics();

            // Using standard open
            using (var db = RocksDb.Open(options, path))
            {
                // With strings
                string value = db.Get("key");
                db.Put("key", "value");
                Assert.Equal("value", db.Get("key"));
                Assert.Null(db.Get("non-existent-key"));
                db.Remove("key");
                Assert.Null(db.Get("value"));

                // With bytes
                db.Put(Encoding.UTF8.GetBytes("key"), Encoding.UTF8.GetBytes("value"));
                Assert.True(BinaryComparer.Default.Equals(Encoding.UTF8.GetBytes("value"), db.Get(Encoding.UTF8.GetBytes("key"))));
                // non-existent kiey
                Assert.Null(db.Get(new byte[] { 0, 1, 2 }));
                db.Remove(Encoding.UTF8.GetBytes("key"));
                Assert.Null(db.Get(Encoding.UTF8.GetBytes("key")));

                db.Put(Encoding.UTF8.GetBytes("key"), new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });

                // With buffers
                var buffer = new byte[100];
                long length = db.Get(Encoding.UTF8.GetBytes("key"), buffer, 0, buffer.Length);
                Assert.Equal(8, length);
                Assert.Equal(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }, buffer.Take((int)length).ToList());

                buffer = new byte[5];
                length = db.Get(Encoding.UTF8.GetBytes("key"), buffer, 0, buffer.Length);
                Assert.Equal(8, length);
                Assert.Equal(new byte[] { 0, 1, 2, 3, 4 }, buffer.Take((int)Math.Min(buffer.Length, length)));

                length = db.Get(Encoding.UTF8.GetBytes("bogus"), buffer, 0, buffer.Length);
                Assert.Equal(-1, length);

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
                Assert.Equal("uno", db.Get("one"));

                // With save point
                using (WriteBatch batch = new WriteBatch())
                {
                    batch
                        .Put("hearts", "red")
                        .Put("diamonds", "red");
                    batch.SetSavePoint();
                    batch
                        .Put("clubs", "black");
                    batch.SetSavePoint();
                    batch
                        .Put("spades", "black");
                    batch.RollbackToSavePoint();
                    db.Write(batch);
                }
                Assert.Equal("red", db.Get("diamonds"));
                Assert.Equal("black", db.Get("clubs"));
                Assert.Null(db.Get("spades"));

                // Save a checkpoint
                using (var cp = db.Checkpoint())
                {
                    cp.Save(cppath);
                }

                // With bytes
                var utf8 = Encoding.UTF8;
                using (WriteBatch batch = new WriteBatch()
                    .Put(utf8.GetBytes("four"), new byte[] { 4, 4, 4 })
                    .Put(utf8.GetBytes("five"), new byte[] { 5, 5, 5 }))
                {
                    db.Write(batch);
                }
                Assert.True(BinaryComparer.Default.Equals(new byte[] { 4, 4, 4 }, db.Get(utf8.GetBytes("four"))));

                // Snapshots
                using (var snapshot = db.CreateSnapshot())
                {
                    var before = db.Get("one");
                    db.Put("one", "1");

                    var useSnapshot = new ReadOptions()
                        .SetSnapshot(snapshot);

                    // the database value was written
                    Assert.Equal("1", db.Get("one"));
                    // but the snapshot still sees the old version
                    var after = db.Get("one", readOptions: useSnapshot);
                    Assert.Equal(before, after);
                }

                var two = db.Get("two");
                Assert.Equal("dos", two);

                // Iterators
                using (var iterator = db.NewIterator(
                    readOptions: new ReadOptions()
                        .SetIterateUpperBound("t")
                        ))
                {
                    iterator.Seek("k");
                    Assert.True(iterator.Valid());
                    Assert.Equal("key", iterator.StringKey());
                    iterator.Next();
                    Assert.True(iterator.Valid());
                    Assert.Equal("one", iterator.StringKey());
                    Assert.Equal("1", iterator.StringValue());
                    iterator.Next();
                    Assert.False(iterator.Valid());
                }

                // MultiGet
                var multiGetResult = db.MultiGet(new[] { "two", "three", "nine" });
                Assert.Equal(
                    expected: new[]
                    {
                        new KeyValuePair<string, string>("two", "dos"),
                        new KeyValuePair<string, string>("three", "tres"),
                        new KeyValuePair<string, string>("nine", null)
                    },
                    actual: multiGetResult
                );
            }

            // Test reading checkpointed db
            using (var cpdb = RocksDb.Open(options, cppath))
            {
                Assert.Equal("red", cpdb.Get("diamonds"));
                Assert.Equal("black", cpdb.Get("clubs"));
                Assert.Null(cpdb.Get("spades"));
                // Checkpoint occurred before these changes:
                Assert.Null(cpdb.Get("four"));
            }

            // Test various operations
            using (var db = RocksDb.Open(options, path))
            {
                // Nulls should be allowed here
                db.CompactRange((byte[])null, (byte[])null);
                db.CompactRange((string)null, (string)null);
            }

            // Test with column families
            var optionsCf = new DbOptions()
                .SetCreateIfMissing(true)
                .SetCreateMissingColumnFamilies(true);

            var columnFamilies = new ColumnFamilies
                {
                    { "reverse", new ColumnFamilyOptions() },
                };

            using (var db = RocksDb.Open(optionsCf, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");

                db.Put("one", "uno");
                db.Put("two", "dos");
                db.Put("three", "tres");

                db.Put("uno", "one", cf: reverse);
                db.Put("dos", "two", cf: reverse);
                db.Put("tres", "three", cf: reverse);
            }

            // Test Cf Delete
            using (var db = RocksDb.Open(optionsCf, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");

                db.Put("cuatro", "four", cf: reverse);
                db.Put("cinco", "five", cf: reverse);

                Assert.Equal("four", db.Get("cuatro", cf: reverse));
                Assert.Equal("five", db.Get("cinco", cf: reverse));

                byte[] keyBytes = Encoding.UTF8.GetBytes("cuatro");
                db.Remove(keyBytes, reverse);
                db.Remove("cinco", reverse);

                Assert.Null(db.Get("cuatro", cf: reverse));
                Assert.Null(db.Get("cinco", cf: reverse));
            }

            // Test list
            {
                var list = RocksDb.ListColumnFamilies(optionsCf, path);
                Assert.Equal(new[] { "default", "reverse" }, list.ToArray());
            }

            // Test reopen with column families
            using (var db = RocksDb.Open(optionsCf, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");

                Assert.Equal("uno", db.Get("one"));
                Assert.Equal("one", db.Get("uno", cf: reverse));
                Assert.Null(db.Get("uno"));
                Assert.Null(db.Get("one", cf: reverse));
            }

            // Test dropping and creating column family
            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                db.DropColumnFamily("reverse");
                var reverse = db.CreateColumnFamily(new ColumnFamilyOptions(), "reverse");
                Assert.Null(db.Get("uno", cf: reverse));
                db.Put("red", "rouge", cf: reverse);
                Assert.Equal("rouge", db.Get("red", cf: reverse));
            }

            // Test reopen after drop and create
            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");
                Assert.Null(db.Get("uno", cf: reverse));
                Assert.Equal("rouge", db.Get("red", cf: reverse));
            }

            // Test read only
            using (var db = RocksDb.OpenReadOnly(options, path, columnFamilies, false))
            {
                Assert.Equal("uno", db.Get("one"));
            }

            // Test SstFileWriter
            {
                using (var writer = new SstFileWriter())
                {
                }

                var envOpts = new EnvOptions();
                var ioOpts = new ColumnFamilyOptions();
                using (var sst = new SstFileWriter(envOpts, ioOpts))
                {
                    var filename = Path.Combine(temp, "test.sst");
                    if (File.Exists(filename))
                        File.Delete(filename);
                    sst.Open(filename);
                    sst.Add("four", "quatro");
                    sst.Add("one", "uno");
                    sst.Add("two", "dos");
                    sst.Finish();

                    using (var db = RocksDb.Open(options, path, columnFamilies))
                    {
                        Assert.NotEqual("four", db.Get("four"));
                        var ingestOptions = new IngestExternalFileOptions()
                            .SetMoveFiles(true);
                        db.IngestExternalFiles(new string[] { filename }, ingestOptions);
                        Assert.Equal("quatro", db.Get("four"));
                    }
                }
            }

            // test comparator
            unsafe {
                var opts = new ColumnFamilyOptions()
                    .SetComparator(new IntegerStringComparator());

                var filename = Path.Combine(temp, "test.sst");
                if (File.Exists(filename))
                    File.Delete(filename);
                using (var sst = new SstFileWriter(ioOptions: opts))
                {
                    sst.Open(filename);
                    sst.Add("111", "111");
                    sst.Add("1001", "1001"); // this order is only allowed using an integer comparator
                    sst.Finish();
                }
            }

            // test write batch with index
            {
                var wbwi = new WriteBatchWithIndex(reservedBytes: 1024);
                wbwi.Put("one", "un");
                wbwi.Put("two", "deux");
                var oneValueIn = Encoding.UTF8.GetBytes("one");
                var oneValueOut = wbwi.Get("one");
                Assert.Equal("un", oneValueOut);
                using (var db = RocksDb.Open(options, path, columnFamilies))
                {
                    var oneCombinedOut = wbwi.Get(db, "one");
                    var threeCombinedOut = wbwi.Get(db, "three");
                    Assert.Equal("un", oneCombinedOut);
                    Assert.Equal("tres", threeCombinedOut);

                    using (var wbIterator = wbwi.NewIterator(db.NewIterator()))
                    {
                        wbIterator.Seek("o");
                        Assert.True(wbIterator.Valid());
                        var itkey = wbIterator.StringKey();
                        Assert.Equal("one", itkey);
                        var itval = wbIterator.StringValue();
                        Assert.Equal("un", itval);

                        wbIterator.Next();
                        Assert.True(wbIterator.Valid());
                        itkey = wbIterator.StringKey();
                        Assert.Equal("three", itkey);
                        itval = wbIterator.StringValue();
                        Assert.Equal("tres", itval);

                        wbIterator.Next();
                        Assert.True(wbIterator.Valid());
                        itkey = wbIterator.StringKey();
                        Assert.Equal("two", itkey);
                        itval = wbIterator.StringValue();
                        Assert.Equal("deux", itval);

                        wbIterator.Next();
                        Assert.False(wbIterator.Valid());
                    }

                    db.Write(wbwi);

                    var oneDbOut = wbwi.Get("one");
                    Assert.Equal("un", oneDbOut);
                }
            }

            // compact range
            {
                using (var db = RocksDb.Open(options, path, columnFamilies))
                {
                    db.CompactRange("o", "tw");
                }
            }

            // Test that GC does not cause access violation on Comparers
            {
                if (Directory.Exists("test-av-error"))
                    Directory.Delete("test-av-error", true);
                options = new RocksDbSharp.DbOptions()
                  .SetCreateIfMissing(true)
                  .SetCreateMissingColumnFamilies(true);
                var sc = new RocksDbSharp.StringComparator(StringComparer.InvariantCultureIgnoreCase);
                columnFamilies = new RocksDbSharp.ColumnFamilies
                {
                     { "cf1", new RocksDbSharp.ColumnFamilyOptions()
                        .SetComparator(sc)
                    },
                };
                GC.Collect();
                using (var db = RocksDbSharp.RocksDb.Open(options, "test-av-error", columnFamilies))
                {
                }
                if (Directory.Exists("test-av-error"))
                    Directory.Delete("test-av-error", true);
            }

            // Smoke test various options
            {
                var dbname = "test-options";
                if (Directory.Exists(dbname))
                    Directory.Delete(dbname, true);
                var optsTest = (DbOptions)new RocksDbSharp.DbOptions()
                  .SetCreateIfMissing(true)
                  .SetCreateMissingColumnFamilies(true)
                  .SetBlockBasedTableFactory(new BlockBasedTableOptions().SetBlockCache(Cache.CreateLru(1024 * 1024)));
                GC.Collect();
                using (var db = RocksDbSharp.RocksDb.Open(optsTest, dbname))
                {
                }
                if (Directory.Exists(dbname))
                    Directory.Delete(dbname, true);

            }

            // Smoke test OpenWithTtl
            {
                var dbname = "test-with-ttl";
                if (Directory.Exists(dbname))
                    Directory.Delete(dbname, true);
                var optsTest = (DbOptions)new RocksDbSharp.DbOptions()
                  .SetCreateIfMissing(true)
                  .SetCreateMissingColumnFamilies(true);
                using (var db = RocksDbSharp.RocksDb.OpenWithTtl(optsTest, dbname, 1))
                {
                }
                if (Directory.Exists(dbname))
                    Directory.Delete(dbname, true);
            }

            // Smoke test MergeOperator
            {
                var dbname = "test-merge-operator";
                if (Directory.Exists(dbname))
                    Directory.Delete(dbname, true);
                var optsTest = (DbOptions)new RocksDbSharp.DbOptions()
                  .SetCreateIfMissing(true)
                  .SetMergeOperator(MergeOperators.Create(
                      name: "test-merge-operator",
                      partialMerge: (key, keyLength, operandsList, operandsListLength, numOperands, success, newValueLength) => IntPtr.Zero,
                      fullMerge: (key, keyLength, existingValue, existingValueLength, operandsList, operandsListLength, numOperands, success, newValueLength) => IntPtr.Zero,
                      deleteValue: (value, valueLength) => { }
                  ));
                GC.Collect();
                using (var db = RocksDbSharp.RocksDb.Open(optsTest, dbname))
                {
                }
                if (Directory.Exists(dbname))
                    Directory.Delete(dbname, true);

            }

            // Test that GC does not cause access violation on Comparers
            {
                var dbname = "test-av-error";
                if (Directory.Exists(dbname))
                    Directory.Delete(dbname, true);
                options = new RocksDbSharp.DbOptions()
                  .SetCreateIfMissing(true)
                  .SetCreateMissingColumnFamilies(true);
                var sc = new RocksDbSharp.StringComparator(StringComparer.InvariantCultureIgnoreCase);
                columnFamilies = new RocksDbSharp.ColumnFamilies
                {
                     { "cf1", new RocksDbSharp.ColumnFamilyOptions()
                        .SetComparator(sc)
                    },
                };
                GC.Collect();
                using (var db = RocksDbSharp.RocksDb.Open(options, dbname, columnFamilies))
                {
                }
                if (Directory.Exists(dbname))
                    Directory.Delete(dbname, true);
            }

        }

        class IntegerStringComparator : StringComparatorBase
        {
            Comparison<long> Comparer { get; } = Comparer<long>.Default.Compare;

            public override int Compare(string a, string b)
                => Comparer(long.TryParse(a, out long avalue) ? avalue : 0, long.TryParse(b, out long bvalue) ? bvalue : 0);
        }
    }
}
