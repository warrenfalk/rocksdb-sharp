using System;
using System.Runtime.InteropServices;

namespace RocksDbSharp
{
    public interface IMergeOperator
    {
        IntPtr Handle { get; }
    }

    public abstract class MergeOperatorBase : IMergeOperator, IDisposable
    {
        public IntPtr Handle { get; private set; }
        private GCHandle gcHandle;
        public string Name { get; private set; }
        private IntPtr NamePtr { get; set; }

        protected unsafe MergeOperatorBase(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            gcHandle = GCHandle.Alloc(this);
            Handle = Native.Instance.rocksdb_mergeoperator_create(GCHandle.ToIntPtr(gcHandle), CallbackDestructor, CallbackFullMerge, CallbackPartialMerge, CallbackDeleteValue, CallbackName);
            NamePtr = Marshal.StringToHGlobalAnsi(Name);
        }

        protected virtual void OnDestroy()
        {
            Handle = IntPtr.Zero;
            gcHandle.Free();
            Marshal.FreeHGlobal(NamePtr);
            NamePtr = IntPtr.Zero;
        }
        protected abstract byte[] OnFullMerge(byte[] key, byte[] existingValue, byte[][] operands, out bool success);
        protected abstract byte[] OnPartialMerge(byte[] key, byte[][] operands, out bool success);

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
                Native.Instance.rocksdb_mergeoperator_destroy(Handle);
        }

        private static void CallbackDestructor(IntPtr target) => ((MergeOperatorBase)GCHandle.FromIntPtr(target).Target).OnDestroy();

        private unsafe static byte* CallbackFullMerge(IntPtr target, byte* key, UIntPtr keyLength, byte* existingValue, UIntPtr existingValueLength, byte** operandsList, UIntPtr* operandsLengthList, int operandsCount, out bool success, out UIntPtr newValueLength)
        {
            var keyArr = new byte[(uint)keyLength];

            Marshal.Copy((IntPtr)key, keyArr, 0, keyArr.Length);
            byte[] existingValueArr = null;
            if ((IntPtr)existingValue != IntPtr.Zero)
            {
                existingValueArr = new byte[(uint)existingValueLength];
                Marshal.Copy((IntPtr)existingValue, existingValueArr, 0, existingValueArr.Length);
            }

            var operands = new byte[operandsCount][];
            for (int i = 0; i < operandsCount; i++)
            {
                var operand = new byte[(uint)operandsLengthList[i]];
                Marshal.Copy((IntPtr)operandsList[i], operand, 0, operand.Length);
                operands[i] = operand;
            }

            var ret = ((MergeOperatorBase)GCHandle.FromIntPtr(target).Target).OnFullMerge(keyArr, existingValueArr, operands, out success);
            var arr = Marshal.AllocHGlobal(ret.Length);
            Marshal.Copy(ret, 0, arr, ret.Length);
            newValueLength = (UIntPtr)ret.Length;
            return (byte*)arr;
        }

        private unsafe static byte* CallbackPartialMerge(IntPtr target, byte* key, UIntPtr keyLength, byte** operandsList, UIntPtr* operandsLengthList, int operandsCount, out bool success, out UIntPtr newValueLength)
        {
            var keyArr = new byte[(uint)keyLength];
            Marshal.Copy((IntPtr)key, keyArr, 0, keyArr.Length);

            var operands = new byte[operandsCount][];
            for (int i = 0; i < operandsCount; i++)
            {
                var operand = new byte[(uint)operandsLengthList[i]];
                Marshal.Copy((IntPtr)operandsList[i], operand, 0, operand.Length);
                operands[i] = operand;
            }

            var ret = ((MergeOperatorBase)GCHandle.FromIntPtr(target).Target).OnPartialMerge(keyArr, operands, out success);
            var arr = Marshal.AllocHGlobal(ret.Length);
            Marshal.Copy(ret, 0, arr, ret.Length);
            newValueLength = (UIntPtr)ret.Length;
            return (byte*)arr;
        }

        private static void CallbackDeleteValue(IntPtr target, IntPtr value, UIntPtr valueLength) => Marshal.FreeHGlobal(value);

        private static IntPtr CallbackName(IntPtr target) => ((MergeOperatorBase)GCHandle.FromIntPtr(target).Target).NamePtr;
    }
}
