using System;
using System.Collections.Generic;
using System.Text;

namespace RocksDbSharp
{
    public interface MergeOperator
    {
        string Name { get; }
        IntPtr PartialMerge(IntPtr key, UIntPtr keyLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, IntPtr success, IntPtr newValueLength);
        IntPtr FullMerge(IntPtr key, UIntPtr keyLength, IntPtr existingValue, UIntPtr existingValueLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, IntPtr success, IntPtr newValueLength);
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
        /// <param name="keyLength"></param>
        /// <param name="operandsList">the sequence of merge operations to apply, front() first</param>
        /// <param name="operandsListLength"></param>
        /// <param name="numOperands"></param>
        /// <param name="success">Client is responsible for filling the merge result here</param>
        /// <param name="newValueLength"></param>
        /// <returns></returns>
        public delegate IntPtr PartialMergeFunc(IntPtr key, UIntPtr keyLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, IntPtr success, IntPtr newValueLength);
        /// <summary>
        /// Gives the client a way to express the read -> modify -> write semantics.
        /// Called when a Put/Delete is the *existing_value (or nullptr)
        /// </summary>
        /// <param name="key">The key that's associated with this merge operation.</param>
        /// <param name="keyLength"></param>
        /// <param name="existingValue">null indicates that the key does not exist before this op</param>
        /// <param name="existingValueLength"></param>
        /// <param name="operandsList">the sequence of merge operations to apply, front() first.</param>
        /// <param name="operandsListLength"></param>
        /// <param name="numOperands"></param>
        /// <param name="success">Client is responsible for filling the merge result here</param>
        /// <param name="newValueLength"></param>
        /// <returns></returns>
        public delegate IntPtr FullMergeFunc(IntPtr key, UIntPtr keyLength, IntPtr existingValue, UIntPtr existingValueLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, IntPtr success, IntPtr newValueLength);
        public delegate void DeleteValueFunc(IntPtr value, UIntPtr valueLength);

        public static MergeOperator Create(
            string name,
            PartialMergeFunc partialMerge,
            FullMergeFunc fullMerge,
            DeleteValueFunc deleteValue)
        {
            return new MergeOperatorImpl(name, partialMerge, fullMerge, deleteValue);
        }

        private class MergeOperatorImpl : MergeOperator
        {
            public string Name { get; }
            private PartialMergeFunc PartialMerge { get; }
            private FullMergeFunc FullMerge { get; }
            private DeleteValueFunc DeleteValue { get; }

            public MergeOperatorImpl(string name, PartialMergeFunc partialMerge, FullMergeFunc fullMerge, DeleteValueFunc deleteValue)
            {
                Name = name;
                PartialMerge = partialMerge;
                FullMerge = fullMerge;
                DeleteValue = deleteValue;
            }

            IntPtr MergeOperator.PartialMerge(IntPtr key, UIntPtr keyLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, IntPtr success, IntPtr newValueLength)
                => PartialMerge(key, keyLength, operandsList, operandsListLength, numOperands, success, newValueLength);

            IntPtr MergeOperator.FullMerge(IntPtr key, UIntPtr keyLength, IntPtr existingValue, UIntPtr existingValueLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, IntPtr success, IntPtr newValueLength)
                => FullMerge(key, keyLength, existingValue, existingValueLength, operandsList, operandsListLength, numOperands, success, newValueLength);

            void MergeOperator.DeleteValue(IntPtr value, UIntPtr valueLength)
                => DeleteValue(value, valueLength);
        }
    }
}
