using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace RocksDbSharp
{
    // TODO: consider somehow reusing the actual unmanaged comparer
    public class BinaryComparer : IEqualityComparer<byte[]>, IComparer<byte[]>
    {
        public static BinaryComparer Default { get; } = new BinaryComparer();

        public int Compare(byte[] a1, byte[] a2)
        {
            int length = Math.Min(a1.Length, a2.Length);
            unsafe
            {
                fixed (byte* p1 = a1, p2 = a2)
                {
                    byte* c1 = p1, c2 = p2;
                    byte* end = c1 + length;
                    byte* end1 = c1 + a1.Length, end2 = c2 + a2.Length;
                    for (; c1 < end && *c1 == *c2; c1++, c2++) ;
                    if (c1 == end1)
                        return c2 == end2 ? 0 : -1;
                    if (c2 == end2)
                        return 1;
                    return (*c1 < *c2) ? -1 : 1;
                }
            }
        }

        public bool Equals(byte[] a1, byte[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            unsafe
            {
                fixed (byte* p1 = a1, p2 = a2)
                {
                    byte* x1 = p1, x2 = p2;
                    int l = a1.Length;
                    for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                        if (*((long*)x1) != *((long*)x2)) return false;
                    if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                    if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                    if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
                    return true;
                }
            }
        }

        public bool PrefixEquals(byte[] a1, byte[] a2, int prefix)
        {
            if (ReferenceEquals(a1, a2))
                return true;
            prefix = Math.Min(prefix, Math.Max(a1.Length, a2.Length));
            var a1length = Math.Min(prefix, a1.Length);
            var a2length = Math.Min(prefix, a2.Length);
            if (a1 == null || a2 == null || a1length != a2length)
                return false;
            unsafe
            {
                fixed (byte* p1 = a1, p2 = a2)
                {
                    byte* x1 = p1, x2 = p2;
                    int l = a1length;
                    for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                        if (*((long*)x1) != *((long*)x2)) return false;
                    if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                    if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                    if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
                    return true;
                }
            }
        }

        public int GetHashCode(byte[] obj)
        {
            return MurMurHash3.Hash(new MemoryStream(obj));
        }
    }

    /*
    This code is public domain.

    The MurmurHash3 algorithm was created by Austin Appleby and put into the public domain.  See http://code.google.com/p/smhasher/

    This C# variant was authored by
    Elliott B. Edwards and was placed into the public domain as a gist
    Status...Working on verification (Test Suite)
    Set up to run as a LinqPad (linqpad.net) script (thus the ".Dump()" call)
    */
    public static class MurMurHash3
    {
        //Change to suit your needs
        const uint seed = 144;

        // TODO: optimize this to use a byte pointer
        public static int Hash(Stream stream)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            uint h1 = seed;
            uint k1 = 0;
            uint streamLength = 0;

            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte[] chunk = reader.ReadBytes(4);
                while (chunk.Length > 0)
                {
                    streamLength += (uint)chunk.Length;
                    switch (chunk.Length)
                    {
                        case 4:
                            /* Get four bytes from the input into an uint */
                            k1 = (uint)
                                (chunk[0]
                                    | chunk[1] << 8
                                    | chunk[2] << 16
                                    | chunk[3] << 24);

                            /* bitmagic hash */
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;

                            h1 ^= k1;
                            h1 = Rotl32(h1, 13);
                            h1 = h1 * 5 + 0xe6546b64;
                            break;
                        case 3:
                            k1 = (uint)
                                (chunk[0]
                                    | chunk[1] << 8
                                    | chunk[2] << 16);
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;
                        case 2:
                            k1 = (uint)
                                (chunk[0]
                                    | chunk[1] << 8);
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;
                        case 1:
                            k1 = (uint)(chunk[0]);
                            k1 *= c1;
                            k1 = Rotl32(k1, 15);
                            k1 *= c2;
                            h1 ^= k1;
                            break;

                    }
                    chunk = reader.ReadBytes(4);
                }
            }

            // finalization, magic chants to wrap it all up
            h1 ^= streamLength;
            h1 = Fmix(h1);

            unchecked //ignore overflow
            {
                return (int)h1;
            }
        }

        private static uint Rotl32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static uint Fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
    }
}

