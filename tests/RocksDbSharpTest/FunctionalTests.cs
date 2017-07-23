﻿using System;
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
            var testdb = Path.Combine(temp, "functional_test");
            string path = Environment.ExpandEnvironmentVariables(testdb);

            if (Directory.Exists(testdb))
                Directory.Delete(testdb, true);

            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .EnableStatistics()
                .SetMergeOperator(new TestConcatMergeOperator());

            // Using standard open
            using (var db = RocksDb.Open(options, path))
            {
                db.Put("mergable", "a");
                db.Merge("mergable", "b");
                db.Merge("mergable", "c");
                Assert.Equal("abc", db.Get("mergable"));
                db.Remove("mergable");

                db.Merge("mergable2", "a");
                db.Merge("mergable2", "b");
                db.Merge("mergable2", "c");
                Assert.Equal("abc", db.Get("mergable2"));
                db.Remove("mergable2");

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
                var envOpts = new EnvOptions();
                var ioOpts = new ColumnFamilyOptions();
                var sst = new SstFileWriter(envOpts, ioOpts);
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

            // test comparator
            unsafe {
                var comparator = new IntegerStringComparator();

                var opts = new ColumnFamilyOptions()
                    .SetComparator(comparator);

                var filename = Path.Combine(temp, "test.sst");
                if (File.Exists(filename))
                    File.Delete(filename);
                var sst = new SstFileWriter(ioOptions: opts);
                sst.Open(filename);
                sst.Add("111", "111");
                sst.Add("1001", "1001"); // this order is only allowed using an integer comparator
                sst.Finish();
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
        }

        class IntegerStringComparator : StringComparatorBase
        {
            Comparison<long> Comparer { get; } = Comparer<long>.Default.Compare;

            public override int Compare(IntPtr state, string a, string b)
                => Comparer(long.TryParse(a, out long avalue) ? avalue : 0, long.TryParse(b, out long bvalue) ? bvalue : 0);
        }
    }
}
