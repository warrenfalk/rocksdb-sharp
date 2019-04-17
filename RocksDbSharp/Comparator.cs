using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Transitional;

namespace RocksDbSharp
{
    public interface Comparator
    {
        string Name { get; }
        int Compare(IntPtr a, UIntPtr alen, IntPtr b, UIntPtr blen);
    }

    public abstract class StringComparatorBase : Comparator
    {
        public Encoding Encoding { get; }

        public string Name { get; }

        public StringComparatorBase(Encoding encoding = null, string name = null, IntPtr state = default(IntPtr))
        {
            Name = name ?? typeof(StringComparatorBase).Name;
            Encoding = encoding ?? Encoding.UTF8;
        }

        public abstract int Compare(string a, string b);

        public unsafe int Compare(IntPtr a, UIntPtr alen, IntPtr b, UIntPtr blen)
        {
            var astr = Encoding.GetString((byte*)a, (int)alen);
            var bstr = Encoding.GetString((byte*)b, (int)blen);
            return Compare(astr, bstr);
        }
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

        public override int Compare(string a, string b) => CompareFunc(a, b);
    }
}
