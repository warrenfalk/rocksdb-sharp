using System;
using System.IO;
using RocksDbSharp;
using Xunit;
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable ArgumentsStyleLiteral

namespace RocksDbSharpTest
{
    public class LifestyleTest
    {
        [Fact]
        public void DoubleDisposableDoesNotThrow()
        {
            var testdir = Path.Combine(Path.GetTempPath(), "lifestyle_test");
            var testdb = Path.Combine(testdir, "main");
            var path = Environment.ExpandEnvironmentVariables(testdb);

            if (Directory.Exists(testdir))
            {
                Directory.Delete(testdir, recursive: true);
            }

            Directory.CreateDirectory(testdir);

            var options = new DbOptions().SetCreateIfMissing(true).EnableStatistics();

            var db = RocksDb.Open(options, path);

            db.Dispose();

            // throws AccessViolationException, which on my machine crashed the process so hard that XUnit coulnd't cope...
            //
            db.Dispose();
            //
            // got this in Event Viewer though:
            //
            //  Application: dotnet.exe
            //  CoreCLR Version: 4.6.28619.1
            //  Description: The process was terminated due to an internal error in the .NET Runtime at IP 00007FFF39BC5AA3 (00007FFF39A20000) with exit code c0000005.
            //
        }
    }
}