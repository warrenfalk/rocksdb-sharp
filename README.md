# rocksdb-sharp

## RocksDb for C# #
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
var options = new Options()
    .SetCreateIfMissing(true);
using (var db = RocksDb.Open(options, path))
{
    // Using strings below, but can also use byte arrays for both keys and values
	// much care has been taken to minimize buffer copying
    db.Put("key", "value");
    string value = db.Get("key");
    db.Remove("key");
}
```

### Requirements

On Linux and Mac, the snappy library (libsnappy) must be installed.

## Caveats and Warnings:

### 64-bit only
RocksDb is supported only in 64-bit mode. Although I contributed a fix that allows it to compile in 32-bit mode, this is untested and unsupported, may not work at all, and almost certainly will have at least some major issues and should not be attempted in production.

### Non-stable Native
The current version of rocksdb that makes this possible is not yet released and so this will currently build straight off the last commit I selected from the rocksdb master branch. Don't use this in production unless you'd be comfortable using the master branch of rocksdb in production

## Extras

Windows Build Script: Building rocksdb for Windows is hard; this project contains a build script to make it easy.

## Building Native Library

Pre-built native binaries can be downloaded from the releases page.  You may also build them yourself.

(This is not a native C# library and the difficulty of a potential native C# library almost certainly far exceeds any usefulness of such a thing and so is not planned and will probably never exist).

This is now buildable on Windows thanks to the Bing team at Microsoft who are actively using rocksdb.  Rocksdb-sharp should work on any platform provided the native unmanaged library is available.

### Windows Native Build Instructions

#### Prerequisities:
* Git for Windows (specifically, the git bash environment)
* CMake
* Visual Studio 2015 (2012 update 4 might work, but I have not tested it)

#### Build Instructions:
1. Open "Developer Command Prompt for VS2015"
2. Run git's ```bash.exe```
3. cd to the ```native-build``` folder within the repository
4. execute ```./build-rocksdb.sh```

This will create a librocksdb.dll and copy it to the where the .sln file is expecting it to be.  (If you only need to run this in Windows, you can remove the references to the other two platform binaries from the .sln)

### Linux Native Build Instructions

1. ```cd native-build```
2. ```./build-rocksdb.sh```

### Mac Native Build Instructions

1. ```cd native-build```
2. ```./build-rocksdb.sh```

Note: the Mono environment that is most used on a Mac is 32-bit, but Rocksdb is 64-bit only. Although a change I contributed now allows RocksDb to compile in 32 bit, it is not supported and may not work.  You should definitely only run 64-bit Mono to use RocksDb with Mono on Mac.

## TODO

  * Many of the C-API functions imports are still under development.
  * Eventually this will be made availabe by NuGet.
  
