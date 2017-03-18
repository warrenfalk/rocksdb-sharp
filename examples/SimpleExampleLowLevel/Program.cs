/*
Simple Low Level Example.

The low level api has been ported from the rocksdb C API (see Native0.cls in RocksDbSharp project).

This is therefore intended to be a direct port from the c_simple_example.c at
https://github.com/facebook/rocksdb/blob/ccc8c10/examples/c_simple_example.c

*/
using RocksDbSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleExampleLowLevel
{
    class Program
    {
        static string temp = Path.GetTempPath();
        static string DBPath = Environment.ExpandEnvironmentVariables(Path.Combine(temp, "rocksdb_simple_example"));
        static string DBBackupPath = Environment.ExpandEnvironmentVariables(Path.Combine(temp, "rocksdb_simple_example_backup"));

        static void Main(string[] args)
        {
            IntPtr db;
            IntPtr be;
            IntPtr options = Native.Instance.rocksdb_options_create();
            // Optimize RocksDB. This is the easiest way to
            // get RocksDB to perform well
            int cpus = Environment.ProcessorCount;
            Native.Instance.rocksdb_options_increase_parallelism(options, cpus);
            Native.Instance.rocksdb_options_optimize_level_style_compaction(options, 0);
            // create the DB if it's not already present
            Native.Instance.rocksdb_options_set_create_if_missing(options, true);

            // open DB
            IntPtr err = IntPtr.Zero;
            db = Native.Instance.rocksdb_open(options, DBPath, out err);
            Debug.Assert(err == IntPtr.Zero);

            // open Backup Engine that we will use for backing up our database
            be = Native.Instance.rocksdb_backup_engine_open(options, DBBackupPath, out err);
            Debug.Assert(err == IntPtr.Zero);

            // Put key-value
            IntPtr writeoptions = Native.Instance.rocksdb_writeoptions_create();
            string key = "key";
            string value = "value";
            Native.Instance.rocksdb_put(db, writeoptions, key, value,
                        out err);
            Debug.Assert(err == IntPtr.Zero);
            // Get value
            IntPtr readoptions = Native.Instance.rocksdb_readoptions_create();
            string returned_value =
                Native.Instance.rocksdb_get(db, readoptions, key, out err);
            Debug.Assert(err == IntPtr.Zero);
            Debug.Assert(returned_value == "value");

            // create new backup in a directory specified by DBBackupPath
            Native.Instance.rocksdb_backup_engine_create_new_backup(be, db, out err);
            Debug.Assert(err == IntPtr.Zero);

            Native.Instance.rocksdb_close(db);

            // If something is wrong, you might want to restore data from last backup
            IntPtr restore_options = Native.Instance.rocksdb_restore_options_create();
            Native.Instance.rocksdb_backup_engine_restore_db_from_latest_backup(be, DBPath, DBPath,
                                                                restore_options, out err);
            Debug.Assert(err == IntPtr.Zero);
            Native.Instance.rocksdb_restore_options_destroy(restore_options);

            db = Native.Instance.rocksdb_open(options, DBPath, out err);
            Debug.Assert(err == IntPtr.Zero);

            // cleanup
            Native.Instance.rocksdb_writeoptions_destroy(writeoptions);
            Native.Instance.rocksdb_readoptions_destroy(readoptions);
            Native.Instance.rocksdb_options_destroy(options);
            Native.Instance.rocksdb_backup_engine_close(be);
            Native.Instance.rocksdb_close(db);

            OtherExamples();
        }

        static void OtherExamples()
        {
            MultiGetExample();
        }

        static void MultiGetExample()
        {
            // Multiget
            IntPtr db;
            IntPtr options = Native.Instance.rocksdb_options_create();
            int cpus = Environment.ProcessorCount;
            Native.Instance.rocksdb_options_increase_parallelism(options, cpus);
            Native.Instance.rocksdb_options_optimize_level_style_compaction(options, 0);
            // create the DB if it's not already present
            Native.Instance.rocksdb_options_set_create_if_missing(options, true);

            // open DB
            IntPtr err = IntPtr.Zero;
            db = Native.Instance.rocksdb_open(options, DBPath, out err);
            Debug.Assert(err == IntPtr.Zero);

            // Put key-value
            IntPtr writeoptions = Native.Instance.rocksdb_writeoptions_create();
            Native.Instance.rocksdb_put(db, writeoptions, "one", "uno", out err);
            Debug.Assert(err == IntPtr.Zero);
            Native.Instance.rocksdb_put(db, writeoptions, "two", "dos", out err);
            Debug.Assert(err == IntPtr.Zero);
            Native.Instance.rocksdb_put(db, writeoptions, "three", "tres", out err);
            Debug.Assert(err == IntPtr.Zero);
            Native.Instance.rocksdb_put(db, writeoptions, "five", "五", out err);
            Debug.Assert(err == IntPtr.Zero);

            // Get value
            IntPtr readoptions = Native.Instance.rocksdb_readoptions_create();
            var values = Native.Instance.rocksdb_multi_get(db, readoptions, new[] { "two", "four", "five" });

            Debug.Assert(values[2].Value == "五");

            string returned_value =
                Native.Instance.rocksdb_get(db, readoptions, "one", out err);
            Debug.Assert(err == IntPtr.Zero);
            Debug.Assert(returned_value == "uno");

            // cleanup
            Native.Instance.rocksdb_writeoptions_destroy(writeoptions);
            Native.Instance.rocksdb_readoptions_destroy(readoptions);
            Native.Instance.rocksdb_options_destroy(options);
            Native.Instance.rocksdb_close(db);
        }
    }
}
