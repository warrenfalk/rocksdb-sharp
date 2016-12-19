using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public static class TransitionExtensions
    {
        public static long GetLongLength<T>(this T[] array, int dimension) => array.GetLength(dimension);
    }
}
