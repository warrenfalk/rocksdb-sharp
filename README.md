# rocksdb-sharp

## RocksDb for C#
RocksDB is a key-value database with a log-structured-merge design, optimized for flash and RAM storage,
which can be tuned to balance write-, read-, and space-amplification factors.

RocksDB is developed by Facebook and is based on LevelDB.
For more information about RocksDB, visit [RocksDB](http://rocksdb.org/) and on [GitHub](https://github.com/facebook/rocksdb)

This library provides C# bindings for rocksdb, implemented as a wrapper for the native rocksdb DLL (unmanaged C++) via the rocksdb C API.

This is a multi-level binding, 
providing direct access to the C API functions (low level) 
plus some helper wrappers on those to aid in marshaling and exception handling (mid level) 
plus an idiomatic C# class hierarchy for ease of use (high level).

The high level wrapper will be patterned after the RocksJava implementation where possible and appropriate.

### Example (High Level)

```csharp
using (var options = new Options().SetCreateIfMissing(true))
using (var db = RocksDb.Open(options, path))
{
    // Using strings below, but can also use byte arrays for both keys and values
	// much care has been taken to minimize buffer copying
    db.Put("key", "value");
    string value = db.Get("key");
    db.Remove("key");
}
```

## Extras

This project also contains a build script for building the rocksdb library on windows.

## Building Native Library

Rocksdb-sharp relies on a native binary.
Thanks to the Bing team at Microsoft who now also use rocksdb
and have contributed back to the community, there is now a Windows build.
Rocksdb-sharp should work on any platform as long as the native library is available.

### Windows

Required: Git (git bash environment), CMake, Visual Studio 2015 (2012 update 4 may work but is untested).

Open git bash, cd to native-build and run build-rocksdb.sh

### Linux

TODO: add instructions

### Mac

TODO: add instructions

## TODO

  * Many of the C-API functions imports are still under development.
  * Eventually this will be made availabe by NuGet.
  * Libraries for Linux and MacOS need to be built.

