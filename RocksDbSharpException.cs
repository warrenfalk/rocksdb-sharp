using System;
using System.Runtime.InteropServices;

namespace RocksDbSharp
{
    [Serializable]
    public class RocksDbSharpException : Exception
    {
        public RocksDbSharpException(string message)
            : base(message)
        {
        }
    }
}