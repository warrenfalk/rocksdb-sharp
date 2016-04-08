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
            ulong writeBufferSize = 33554432;
            int maxWriteBufferNumber = 2;
            int minWriteBufferNumberToMerge = 1;
            uint memtablePrefixBloomBits = 10000000;
            int memtablePrefixBloomProbes = 10;
            ulong memtablePrefixBloomHugePageTlbSize = 2 * 1024 * 1024;
            
            string temp = Path.GetTempPath();
            string path = Environment.ExpandEnvironmentVariables(Path.Combine(temp, "rocksdb_prefix_example"));
            var bbto = new BlockBasedTableOptions()
                .SetFilterPolicy(BloomFilterPolicy.Create(10, false))
                .SetWholeKeyFiltering(false);
            var options = new DbOptions()
                .SetCreateIfMissing(true)
                .SetWriteBufferSize(writeBufferSize)
                .SetMaxWriteBufferNumber(maxWriteBufferNumber)
                .SetMinWriteBufferNumberToMerge(minWriteBufferNumberToMerge)
                .SetMemtablePrefixBloomBits(memtablePrefixBloomBits)
                .SetMemtablePrefixBloomProbes(memtablePrefixBloomProbes)
                .SetMemtablePrefixBloomHugePageTlbSize(memtablePrefixBloomHugePageTlbSize)
                .SetPrefixExtractor(SliceTransform.CreateFixedPrefix(8))
                .SetBlockBasedTableFactory(bbto);
            using (var db = RocksDb.Open(options, path))
            {
            }
            Console.WriteLine("Done...");
        }
    }
}
