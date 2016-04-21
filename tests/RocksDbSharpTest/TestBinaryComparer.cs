using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RocksDbSharp;
using System.Text;

namespace RocksDbSharpTest
{
    [TestClass]
    public class TestBinaryComparer
    {
        [TestMethod]
        public void TestCompare()
        {
            var comparer = new BinaryComparer();

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

        private void AssertCompare(BinaryComparer comparer, int expected, string v1, string v2)
        {
            Assert.AreEqual(expected, comparer.Compare(Encoding.UTF8.GetBytes(v1), Encoding.UTF8.GetBytes(v2)), string.Format("{0} -> {1}", v1, v2));
        }
    }
}
