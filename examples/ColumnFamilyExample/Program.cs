using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnFamilyExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string temp = Path.GetTempPath();
            string path = Environment.ExpandEnvironmentVariables(Path.Combine(temp, "rocksdb_cf_example"));

            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .SetCreateMissingColumnFamilies(true);

            var columnFamilies = new ColumnFamilies
            {
                { "reverse", new ColumnFamilyOptions() },
            };

            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");

                db.Put("one", "uno");
                db.Put("two", "dos");
                db.Put("three", "tres");

                db.Put("uno", "one", cf: reverse);
                db.Put("dos", "two", cf: reverse);
                db.Put("tres", "three", cf: reverse);
            }

            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");

                string uno = db.Get("one");
                string one = db.Get("uno", cf: reverse);
                string nada;
                nada = db.Get("uno");
                nada = db.Get("one", cf: reverse);
            }

            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                db.DropColumnFamily("reverse");
                var reverse = db.CreateColumnFamily(new ColumnFamilyOptions(), "reverse");
                var nada = db.Get("uno", cf: reverse);
                db.Put("red", "rouge", cf: reverse);
            }

            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                var reverse = db.GetColumnFamily("reverse");
                var nada = db.Get("uno", cf: reverse);
                var rouge = db.Get("red", cf: reverse);
            }

            using (var db = RocksDb.OpenReadOnly(options, path, columnFamilies, false))
            {
                string uno = db.Get("one");
            }
        }
    }
}
