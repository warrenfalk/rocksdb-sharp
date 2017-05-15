using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Transitional;

namespace RocksDbSharp
{
    public unsafe delegate int CompareFunc(IntPtr state, byte* a, ulong alen, byte* b, ulong blen);
    public delegate void DestroyFunc(IntPtr state);
    public delegate string GetNameFunc(IntPtr state);

    public interface Comparator
    {
        IntPtr Handle { get; }
    }

    public abstract class ComparatorBase : Comparator
    {
        public IntPtr Handle { get; protected set; }
        public virtual string Name { get; }

        private DestroyFunc _destroy;

        public unsafe ComparatorBase(string name = null, IntPtr state = default(IntPtr))
        {
            Name = name ?? GetType().FullName;
            _destroy = s => this.Destroy(s);
            Handle = Native.Instance.rocksdb_comparator_create(
                state: IntPtr.Zero,
                destructor: CurrentFramework.GetFunctionPointerForDelegate(_destroy),
                compare: CurrentFramework.GetFunctionPointerForDelegate<CompareFunc>(Compare),
                getName: CurrentFramework.GetFunctionPointerForDelegate<GetNameFunc>(GetName)
            );
        }

        ~ComparatorBase()
        {
            if (Handle != IntPtr.Zero)
            {
#if !NODESTROY
                Native.Instance.rocksdb_comparator_destroy(Handle);
#endif
                Handle = IntPtr.Zero;
            }
        }

        public abstract unsafe int Compare(IntPtr state, byte* a, ulong alen, byte* b, ulong blen);

        public virtual void Destroy(IntPtr state)
        {
        }

        protected virtual string GetName(IntPtr state) => Name;
    }

    public abstract class StringComparatorBase : ComparatorBase
    {
        public Encoding Encoding { get; }

        public StringComparatorBase(Encoding encoding = null, string name = null, IntPtr state = default(IntPtr))
            : base(name, state)
        {
            Encoding = encoding ?? Encoding.UTF8;
        }

        public override unsafe int Compare(IntPtr state, byte* a, ulong alen, byte* b, ulong blen)
        {
            var astr = Encoding.GetString(a, (int)alen);
            var bstr = Encoding.GetString(b, (int)blen);
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
