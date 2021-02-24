using System;
using Xunit;
using System.IO;
using RocksDbSharp;
using System.Linq;

namespace RocksDbSharpTest
{
    public class AdditionalTests
    {
        public const string ReplacedText = "REPLACEMENT_TEXT";

        [Fact]
        public void TestFlush()
        {
            var dbName = "TestFlushDB";

            DeleteDb(dbName);

            var options = new DbOptions().SetCreateIfMissing(true);

            using (var db = RocksDb.Open(options, dbName))
            {
                db.Put("key", "value");
            }

            var sstFiles = Directory.EnumerateFiles(dbName).Where(s => s.EndsWith(".sst", StringComparison.OrdinalIgnoreCase));

            Assert.True(!sstFiles.Any());

            DeleteDb(dbName);

            FlushOptions flushOptions = new FlushOptions().SetWaitForFlush(true);

            using (var db = RocksDb.Open(options, dbName))
            {
                db.Put("key", "value");
                db.Flush(flushOptions);
            }

            sstFiles = Directory.EnumerateFiles(dbName).Where(s => s.EndsWith(".sst", StringComparison.OrdinalIgnoreCase));
            Assert.True(sstFiles.Any());
        }

        [Fact]
        public void TestRepairDB()
        {
            var dbName = "TestFlushDB";
            DeleteDb(dbName);
            var options = new DbOptions().SetCreateIfMissing(true);
            var flushOptions = new FlushOptions().SetWaitForFlush(true);

            using (var db = RocksDb.Open(options, dbName))
            {
                db.Put("key0", "value0");
                db.Flush(flushOptions);
            }

            var firstSSTfile = Directory.EnumerateFiles(dbName).Where(s => s.EndsWith(".sst", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var checkKey = "key1";
            var checkValue = "value0";
            using (var db = RocksDb.Open(options, dbName))
            {
                db.Put(checkKey, checkValue);
                db.Flush(flushOptions);
            }

            File.Delete($"{firstSSTfile}");
            RocksDb.RepairDB(options, dbName);

            using (var db = RocksDb.Open(options, dbName))
            {
                Assert.Equal(db.Get("key0"), null);
                Assert.Equal(db.Get(checkKey), checkValue);
            }
        }

        [Fact]
        public void TestLiveFiles()
        {
            var dbName = "TestLiveFiles";
            DeleteDb(dbName);
            var options = new DbOptions().SetCreateIfMissing(true);
            var flushOptions = new FlushOptions().SetWaitForFlush(true);

            {
                using (var db = RocksDb.Open(options, dbName))
                {
                    var files = db.GetLiveFilesMetadata();

                    Assert.True(files.Count == 0);                    
                }
            }

            {
                using (var db = RocksDb.Open(options, dbName))
                {
                    db.Put("key0", "value0");
                    db.Put("key1", "value0");
                    db.Flush(flushOptions);

                    db.Put("key7", "value0");
                    db.Put("key8", "value0");

                    db.Flush(flushOptions);

                    var files = db.GetLiveFilesMetadata();
                    var fileNames = files.Select(file => file.FileMetadata.FileName);
                    var fileList = Directory.EnumerateFiles(dbName);

                    Assert.True(fileList.All(file => fileList.Contains(file)));
                    Assert.Equal(db.Get("key0"), "value0");
                }
            }            
        }

        [Fact]
        public void TestLiveFileNames()
        {
            var dbName = "TestLiveFiles";
            DeleteDb(dbName);
            var options = new DbOptions().SetCreateIfMissing(true);
            var flushOptions = new FlushOptions().SetWaitForFlush(true);

            using (var db = RocksDb.Open(options, dbName))
            {
                db.Put("key0", "value0");
                db.Put("key1", "value0");
                db.Flush(flushOptions);

                db.Put("key7", "value0");
                db.Put("key8", "value0");

                db.Flush(flushOptions);

                var files = db.GetLiveFileNames();
                var fileList = Directory.EnumerateFiles(dbName);

                Assert.True(fileList.All(file => fileList.Contains(file)));
                Assert.Equal(db.Get("key0"), "value0");
            }
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
