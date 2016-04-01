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
                .SetCreateIfMissing(true);

            var columnFamilies = new ColumnFamilies
            {
                { "new_cf", new ColumnFamilyOptions() },
            };

            using (var db = RocksDb.Open(options, path, columnFamilies))
            {
                var newCf = db.GetColumnFamily("new_cf");
            }
        }
    }
}
