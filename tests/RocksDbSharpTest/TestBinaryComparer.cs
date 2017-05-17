using System;
using Xunit;
using RocksDbSharp;
using System.Text;

namespace RocksDbSharpTest
{
    public class TestBinaryComparer
    {
        [Fact]
        public void TestCompare()
        {
            var comparer = BinaryComparer.Default;

            var forward = StringComparer.OrdinalIgnoreCase.Compare("a", "b");
            var backward = -forward;

            AssertCompare(comparer, forward, "B", "b");
            AssertCompare(comparer, backward, "b", "B");

            AssertCompare(comparer, forward, "aB", "ab");
            AssertCompare(comparer, backward, "ab", "aB");

            AssertCompare(comparer, forward, "cB", "cb");
            AssertCompare(comparer, backward, "cb", "cB");

            AssertCompare(comparer, forward, "b", "bb");
            AssertCompare(comparer, backward, "bb", "b");
        }

        [Fact]
        public void TestPrefixEquals()
        {
            var comparer = BinaryComparer.Default;

            Assert.True(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaa"), 1));
            Assert.True(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaa"), 3));
            Assert.True(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaa"), 5));

            Assert.True(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaX"), 1));
            Assert.True(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaX"), 2));
            Assert.False(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaX"), 3));
            Assert.False(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaX"), 5));

            Assert.True(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaaX"), 1));
            Assert.True(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaaX"), 3));
            Assert.False(comparer.PrefixEquals(AsciiBytes("aaa"), AsciiBytes("aaaX"), 4));

            Assert.True(comparer.PrefixEquals(AsciiBytes("aaaX"), AsciiBytes("aaa"), 1));
            Assert.True(comparer.PrefixEquals(AsciiBytes("aaaX"), AsciiBytes("aaa"), 3));
            Assert.False(comparer.PrefixEquals(AsciiBytes("aaaX"), AsciiBytes("aaa"), 4));
        }

        private byte[] AsciiBytes(string v)
        {
            return Encoding.ASCII.GetBytes(v);
        }

        private void AssertCompare(BinaryComparer comparer, int expected, string v1, string v2)
        {
            Assert.Equal(expected, comparer.Compare(Encoding.UTF8.GetBytes(v1), Encoding.UTF8.GetBytes(v2)));
        }
    }
}
