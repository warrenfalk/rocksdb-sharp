using System;

namespace RocksDbSharp
{
    public interface IRocksDbHandle : IDisposable
    {
        IntPtr Handle { get; }
    }
}