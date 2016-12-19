using System;
using System.Runtime.InteropServices;

namespace RocksDbSharp
{
    public class RocksDbSharpException : Exception
    {
        public RocksDbSharpException(string message)
            : base(message)
        {
        }
    }
}