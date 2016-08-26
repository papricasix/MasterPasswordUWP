using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Windows.Storage.Streams;

namespace MasterPasswordUWP.Algorithm
{
    // https://github.com/Lyndir/MasterPassword/blob/master/MasterPassword/Java/masterpassword-algorithm/src/main/java/com/lyndir/masterpassword/MasterKey.java

    public enum KeyVersion
    {
        /**
         * bugs:
         * - does math with chars whose signedness was platform-dependent.
         * - miscounted the byte-length fromInt multi-byte site names.
         * - miscounted the byte-length fromInt multi-byte full names.
         */
        V0,

        /**
         * bugs:
         * - miscounted the byte-length fromInt multi-byte site names.
         * - miscounted the byte-length fromInt multi-byte full names.
         */
        V1,

        /**
         * bugs:
         * - miscounted the byte-length fromInt multi-byte full names.
         */
        V2,

        /**
         * bugs:
         * - no known issues.
         */
        V3,

        Current = V3
    }

    public static class KeyVersionHelper
    {
        public static string ToBundleVersion(KeyVersion e)
        {
            switch (e)
            {
                case KeyVersion.V0:
                    return "1.0";
                case KeyVersion.V1:
                    return "2.0";
                case KeyVersion.V2:
                    return "2.1";
                case KeyVersion.V3:
                    return "2.2";
            }
            throw new InvalidOperationException($"Unsupported version: {e}");
        }
    }

    public abstract class MasterKey
    {
        public string FullName { get; private set; }
        public byte[] MasterKeyBytes { get; private set; }

        public static MasterKey Create(string fullName, char[] masterPassword)
        {
            return Create(KeyVersion.Current, fullName, masterPassword);
        }

        public static MasterKey Create(KeyVersion version, string fullName, char[] masterPassword)
        {
            switch (version)
            {
                case KeyVersion.V0:
                    return new MasterKeyV0(fullName).Revalidate(masterPassword);
                case KeyVersion.V1:
                    return new MasterKeyV1(fullName).Revalidate(masterPassword);
                case KeyVersion.V2:
                    return new MasterKeyV2(fullName).Revalidate(masterPassword);
                case KeyVersion.V3:
                    return new MasterKeyV3(fullName).Revalidate(masterPassword);
            }
            throw new InvalidOperationException($"Unsupported version: {version}");
        }

        protected MasterKey(string fullName)
        {
            FullName = fullName;
        }

        public MasterKey Revalidate(char[] masterPassword)
        {
            Invalidate();
            MasterKeyBytes = DeriveKey(masterPassword);
            // todo logging?
            return this;
        }

        public void Invalidate()
        {
            if (MasterKeyBytes != null)
            {
                for (var i = 0; i < MasterKeyBytes.Length; ++i)
                {
                    MasterKeyBytes[i] = 0;
                }
                MasterKeyBytes = null;
            }
        }

        public abstract string Encode(string siteName, SiteType siteType, uint siteCounter, SiteVariant siteVariant,
            string siteContext = null);

        protected abstract byte[] DeriveKey(char[] masterPassword);

        public abstract KeyVersion AlgorithmVersion { get; }

        protected abstract byte[] BytesForInt(int number);

        protected abstract byte[] BytesForInt(uint number);

        protected abstract byte[] IdForBytes(byte[] bytes);

        protected static T[] Concat<T>(params T[][] a)
        {
            var r = new List<T>();
            foreach (var e in a)
            {
                r.AddRange(e);
            }
            return r.ToArray();
        }

        public static void Fill<T>(T[] a, T v)
        {
            for (var i = 0; i < a.Length; ++i)
            {
                a[i] = v;
            }
        }

        public static void Fill(Stream stream, byte v)
        {
            var oldPos = stream.Position;
            var w = new BinaryWriter(stream);
            for (var i = oldPos; i < stream.Length; ++i)
            {
                w.Write(v);
            }
            w.Flush();
            stream.Position = oldPos;
        }

        public static void Fill(Stream stream, sbyte v)
        {
            var oldPos = stream.Position;
            var w = new BinaryWriter(stream);
            for (var i = oldPos; i < stream.Length; ++i)
            {
                w.Write(v);
            }
            w.Flush();
            stream.Position = oldPos;
        }

        public static byte[] ComputeHmacOf(byte[] key, byte[] bytes)
        {
            var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(bytes);
        }

        protected static byte[] ChangeByteOrder(byte[] bytes, ByteOrder order)
        {
            if (BitConverter.IsLittleEndian && order != ByteOrder.LittleEndian || !BitConverter.IsLittleEndian && order != ByteOrder.BigEndian)
            {
                return bytes.Reverse().ToArray();
            }
            return bytes;
        }
    }
}
