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

### Example (High Level)

```csharp
var options = new DbOptions()
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
### Usage

#### Using NuGet:

```
install-package RocksDbSharp
```

This will install the managed library which will use the unmanaged library installed on
the machine at runtime.  If you do not want to install the managed library, you can
include it by additionally installing the "RocksDbNative" package.

```
install-package RocksDbNative
```

### Requirements

On Linux and Mac, the snappy library (libsnappy) must be installed.

## Caveats and Warnings:

### 64-bit only (Especially on Windows)
RocksDb is supported only in 64-bit mode. Although I contributed a fix that allows it to compile in 32-bit mode, this is untested and unsupported, may not work at all, and almost certainly will have at least some major issues and should not be attempted in production.

## Extras

Windows Build Script: Building rocksdb for Windows is hard; this project contains a build script to make it easy.

## Building Native Library

Pre-built native binaries can be downloaded from the releases page.  You may also build them yourself.

(This is only a high level (and low level) wrapper on the unmanaged library and is not a managed C# port of the rocksdb database. The difficulty of a potential managed port library almost certainly far exceeds any usefulness of such a thing and so is not planned and will probably never exist).

This is now buildable on Windows thanks to the Bing team at Microsoft who are actively using rocksdb.  Rocksdb-sharp should work on any platform provided the native unmanaged library is available.

### Windows Native Build Instructions

#### Prerequisities:
* Git for Windows (specifically, the git bash environment)
* CMake
* Visual Studio 2017 (older versions may work but are not tested)

#### Build Instructions:
1. Open "Developer Command Prompt for VS2017"
2. Run git's ```bash.exe```
3. cd to the ```native-build``` folder within the repository
4. execute ```./build-rocksdb.sh```

This will create a rocksdb.dll and copy it to the where the .sln file is expecting it to be.  (If you only need to run this in Windows, you can remove the references to the other two platform binaries from the .sln)

### Linux Native Build Instructions

1. ```cd native-build```
2. ```./build-rocksdb.sh```

### Mac Native Build Instructions

1. ```cd native-build```
2. ```./build-rocksdb.sh```

Note: On a Mac, although a change I contributed now allows RocksDb to compile in 32 bit, it is not supported and may not work.  You should definitely only run 64-bit Mono to use RocksDb with Mono on Mac.

## TODO

  * Many of the less-commonly-used C-API functions imports are yet to be included.
  
