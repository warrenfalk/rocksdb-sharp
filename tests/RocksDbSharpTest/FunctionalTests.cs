using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using RocksDbSharp;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace RocksDbSharpTest
{
    [TestClass]
    public class FunctionalTests
    {
        [TestMethod]
        public void FunctionalTest()
        {
            string temp = Path.GetTempPath();
            var testdb = Path.Combine(temp, "functional_test");
            string path = Environment.ExpandEnvironmentVariables(testdb);

            if (Directory.Exists(testdb))
                Directory.Delete(testdb, true);

            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .EnableStatistics();

            // Using standard open
            using (var db = RocksDb.Open(options, path))
            {
                // With strings
                string value = db.Get("key");
                db.Put("key", "value");
                Assert.AreEqual("value", db.Get("key"));
                Assert.IsNull(db.Get("non-existent-key"));
                db.Remove("key");
                Assert.IsNull(db.Get("value"));

                // With bytes
                db.Put(Encoding.UTF8.GetBytes("key"), Encoding.UTF8.GetBytes("value"));
                Assert.IsTrue(BinaryComparer.Default.Equals(Encoding.UTF8.GetBytes("value"), db.Get(Encoding.UTF8.GetBytes("key"))));
                // non-existent kiey
                Assert.IsNull(db.Get(new byte[] { 0, 1, 2 }));
                db.Remove(Encoding.UTF8.GetBytes("key"));
                Assert.IsNull(db.Get(Encoding.UTF8.GetBytes("key")));

                db.Put(Encoding.UTF8.GetBytes("key"), new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });

                // With buffers
                var buffer = new byte[100];
                long length = db.Get(Encoding.UTF8.GetBytes("key"), buffer, 0, buffer.Length);
                Assert.AreEqual(8, length);
                CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }, buffer.Take((int)length).ToList());

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
                Assert.AreEqual("uno", db.Get("one"));

                // With bytes
                var utf8 = Encoding.UTF8;
                using (WriteBatch batch = new WriteBatch()
                    .Put(utf8.GetBytes("four"), new byte[] { 4, 4, 4 })
                    .Put(utf8.GetBytes("five"), new byte[] { 5, 5, 5 }))
                {
                    db.Write(batch);
                }
                Assert.IsTrue(BinaryComparer.Default.Equals(new byte[] { 4, 4, 4 }, db.Get(utf8.GetBytes("four"))));

                // Snapshots
                using (var snapshot = db.CreateSnapshot())
                {
                    var before = db.Get("one");
                    db.Put("one", "1");

                    var useSnapshot = new ReadOptions()
                        .SetSnapshot(snapshot);

                    // the database value was written
                    Assert.AreEqual("1", db.Get("one"));
                    // but the snapshot still sees the old version
                    var after = db.Get("one", readOptions: useSnapshot);
                    Assert.AreEqual(before, after);
                }

                var two = db.Get("two");
                Assert.AreEqual("dos", two);

                // Iterators
                using (var iterator = db.NewIterator(
                    readOptions: new ReadOptions()
                        .SetIterateUpperBound("t")
                        ))
                {
                    iterator.Seek("k");
                    Assert.IsTrue(iterator.Valid());
                    Assert.AreEqual("key", iterator.StringKey());
                    iterator.Next();
                    Assert.IsTrue(iterator.Valid());
                    Assert.AreEqual("one", iterator.StringKey());
                    Assert.AreEqual("1", iterator.StringValue());
                    iterator.Next();
                    Assert.IsFalse(iterator.Valid());
                }

                // MultiGet
                var multiGetResult = db.MultiGet(new[] { "two", "three", "nine" });
                CollectionAssert.AreEqual(
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

            // Test reopen with column families
            using (var db = RocksDb.Open(optionsCf, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");

                Assert.AreEqual("uno", db.Get("one"));
                Assert.AreEqual("one", db.Get("uno", cf: reverse));
                Assert.IsNull(db.Get("uno"));
                Assert.IsNull(db.Get("one", cf: reverse));
            }

            // Test dropping and creating column family
            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                db.DropColumnFamily("reverse");
                var reverse = db.CreateColumnFamily(new ColumnFamilyOptions(), "reverse");
                Assert.IsNull(db.Get("uno", cf: reverse));
                db.Put("red", "rouge", cf: reverse);
                Assert.AreEqual("rouge", db.Get("red", cf: reverse));
            }

            // Test reopen after drop and create
            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");
                Assert.IsNull(db.Get("uno", cf: reverse));
                Assert.AreEqual("rouge", db.Get("red", cf: reverse));
            }

            // Test read only
            using (var db = RocksDb.OpenReadOnly(options, path, columnFamilies, false))
            {
                Assert.AreEqual("uno", db.Get("one"));
            }
        }
    }
}
