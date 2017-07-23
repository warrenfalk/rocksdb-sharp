using System;
using System.Linq;
using RocksDbSharp;

namespace RocksDbSharpTest
{
    public sealed class TestConcatMergeOperator : MergeOperatorBase
    {
        public TestConcatMergeOperator() : base("CONCAT") { }

        protected override byte[] OnFullMerge(byte[] key, byte[] existingValue, byte[][] operands, out bool success)
        {
            var pos = 0;
            var arr = new byte[(existingValue?.Length ?? 0) + operands.Sum(x => x.Length)];
            if (existingValue != null)
            {
                Buffer.BlockCopy(existingValue, 0, arr, 0, existingValue.Length);
                pos = existingValue.Length;
            }
            foreach (var operand in operands)
            {
                Buffer.BlockCopy(operand, 0, arr, pos, operand.Length);
                pos += operand.Length;
            }
            success = true;
            return arr;
        }

        protected override byte[] OnPartialMerge(byte[] key, byte[][] operands, out bool success)
        {
            var arr = new byte[operands.Sum(x => x.Length)];
            var pos = 0;
            foreach (var operand in operands)
            {
                Buffer.BlockCopy(operand, 0, arr, pos, operand.Length);
                pos += operand.Length;
            }
            success = true;
            return arr;
        }
    }
}
