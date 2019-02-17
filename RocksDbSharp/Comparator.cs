using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Transitional;

namespace RocksDbSharp
{
    public interface Comparator
    {
        IntPtr Handle { get; }
    }

    public abstract class ComparatorBase : Comparator
    {
        public IntPtr Handle { get; protected set; }
        public virtual string Name { get; }
        private IntPtr NamePtr { get; }

        public unsafe ComparatorBase(string name = null, IntPtr state = default(IntPtr))
        {
            Name = name ?? GetType().FullName;
            var nameBytes = Encoding.UTF8.GetBytes(name + "\0");
            NamePtr = Marshal.AllocHGlobal(nameBytes.Length);
            Marshal.Copy(nameBytes, 0, NamePtr, nameBytes.Length);
            Handle = Native.Instance.rocksdb_comparator_create(
                state: IntPtr.Zero,
                destructor: Destroy,
                compare: Compare,
                name: GetNamePtr
            );
        }

        ~ComparatorBase()
        {
            Marshal.FreeHGlobal(NamePtr);
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_comparator_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }

        public abstract unsafe int Compare(IntPtr state, IntPtr a, UIntPtr alen, IntPtr b, UIntPtr blen);

        public virtual void Destroy(IntPtr state)
        {
        }

        private IntPtr GetNamePtr(IntPtr state) => NamePtr;
    }

    public abstract class StringComparatorBase : ComparatorBase
    {
        public Encoding Encoding { get; }

        public StringComparatorBase(Encoding encoding = null, string name = null, IntPtr state = default(IntPtr))
            : base(name, state)
        {
            Encoding = encoding ?? Encoding.UTF8;
        }

        public override unsafe int Compare(IntPtr state, IntPtr a, UIntPtr alen, IntPtr b, UIntPtr blen)
        {
            var astr = Encoding.GetString((byte*)a, (int)alen);
            var bstr = Encoding.GetString((byte*)b, (int)blen);
            return Compare(state, astr, bstr);
        }

        public abstract int Compare(IntPtr state, string a, string b);
    }

    public class StringComparator : StringComparatorBase
    {
        public Comparison<string> CompareFunc { get; }

        public StringComparator(IComparer<string> comparer = null, Encoding encoding = null, string name = null)
            : base(encoding, name)
        {
            if (comparer == null)
                comparer = StringComparer.CurrentCulture;
            CompareFunc = comparer.Compare;
        }

        public StringComparator(bool ignoreCase, Encoding encoding = null, string name = null)
            : this(ignoreCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture, encoding, name)
        {
        }

        public override int Compare(IntPtr state, string a, string b) => CompareFunc(a, b);
    }
}
