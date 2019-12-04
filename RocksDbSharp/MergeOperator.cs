using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RocksDbSharp
{
    public interface MergeOperator
    {
        string Name { get; }
        IntPtr PartialMerge(IntPtr key, UIntPtr keyLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, out IntPtr success, out IntPtr newValueLength);
        IntPtr FullMerge(IntPtr key, UIntPtr keyLength, IntPtr existingValue, UIntPtr existingValueLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, out IntPtr success, out IntPtr newValueLength);
        void DeleteValue(IntPtr value, UIntPtr valueLength);
    }

    public static class MergeOperators
    {
        /// <summary>
        /// This function performs merge(left_op, right_op)
        /// when both the operands are themselves merge operation types.
        /// Save the result in *new_value and return true. If it is impossible
        /// or infeasible to combine the two operations, return false instead.
        /// This is called to combine two-merge operands (if possible)
        /// </summary>
        /// <param name="key">The key that's associated with this merge operation</param>
        /// <param name="operands">the sequence of merge operations to apply, front() first</param>
        /// <param name="success">Client is responsible for filling the merge result here</param>
        /// <returns></returns>
        public delegate byte[] PartialMergeFunc(byte[] key, byte[][] operands, out bool success);

        /// <summary>
        /// Gives the client a way to express the read -> modify -> write semantics.
        /// Called when a Put/Delete is the *existing_value (or nullptr)
        /// </summary>
        /// <param name="key">The key that's associated with this merge operation.</param>
        /// <param name="existingValue">null indicates that the key does not exist before this op</param>
        /// <param name="operands">the sequence of merge operations to apply, front() first.</param>
        /// <param name="success">Client is responsible for filling the merge result here</param>
        /// <returns></returns>
        public delegate byte[] FullMergeFunc(byte[] key, byte[] existingValue, byte[][] operands, out bool success);

        public static MergeOperator Create(
            string name,
            PartialMergeFunc partialMerge,
            FullMergeFunc fullMerge)
        {
            return new MergeOperatorImpl(name, partialMerge, fullMerge);
        }

        private class MergeOperatorImpl : MergeOperator
        {
            public string Name { get; }
            private PartialMergeFunc PartialMerge { get; }
            private FullMergeFunc FullMerge { get; }

            public MergeOperatorImpl(string name, PartialMergeFunc partialMerge, FullMergeFunc fullMerge)
            {
                Name = name;
                PartialMerge = partialMerge;
                FullMerge = fullMerge;
            }

            IntPtr MergeOperator.PartialMerge(IntPtr key, UIntPtr keyLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, out IntPtr success, out IntPtr newValueLength)
            {
                var _key = new byte[(uint)keyLength];
                Marshal.Copy(key, _key, 0, _key.Length);

                var _operandsList = new IntPtr[numOperands];
                Marshal.Copy(operandsList, _operandsList, 0, _operandsList.Length);

                var _operandsListLength = new long[numOperands];
                Marshal.Copy(operandsListLength, _operandsListLength, 0, _operandsListLength.Length);

                var operands = new byte[numOperands][];
                for (int i = 0; i < numOperands; i++)
                {
                    var operand = new byte[_operandsListLength[i]];
                    Marshal.Copy(_operandsList[i], operand, 0, operand.Length);
                    operands[i] = operand;
                }

                var value = PartialMerge(_key, operands, out var _success);

                var ret = Marshal.AllocHGlobal(value.Length);
                Marshal.Copy(value, 0, ret, value.Length);
                newValueLength = (IntPtr)value.Length;

                success = (IntPtr)Convert.ToInt32(_success);

                return ret;
            }

            IntPtr MergeOperator.FullMerge(IntPtr key, UIntPtr keyLength, IntPtr existingValue, UIntPtr existingValueLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, out IntPtr success, out IntPtr newValueLength)
            {
                var _key = new byte[(uint)keyLength];
                Marshal.Copy(key, _key, 0, _key.Length);

                byte[] _existingValue = null;
                if (existingValue != IntPtr.Zero)
                {
                    _existingValue = new byte[(uint)existingValueLength];
                    Marshal.Copy(existingValue, _existingValue, 0, _existingValue.Length);
                }

                var _operandsList = new IntPtr[numOperands];
                Marshal.Copy(operandsList, _operandsList, 0, _operandsList.Length);

                var _operandsListLength = new long[numOperands];
                Marshal.Copy(operandsListLength, _operandsListLength, 0, _operandsListLength.Length);

                var operands = new byte[numOperands][];

                for (int i = 0; i < numOperands; i++)
                {
                    var operand = new byte[_operandsListLength[i]];
                    Marshal.Copy(_operandsList[i], operand, 0, operand.Length);
                    operands[i] = operand;
                }

                var value = FullMerge(_key, _existingValue, operands, out var _success);

                var ret = Marshal.AllocHGlobal(value.Length);
                Marshal.Copy(value, 0, ret, value.Length);
                newValueLength = (IntPtr)value.Length;

                success = (IntPtr)Convert.ToInt32(_success);

                return ret;
            }

            void MergeOperator.DeleteValue(IntPtr value, UIntPtr valueLength) => Marshal.FreeHGlobal(value);
        }
    }
}