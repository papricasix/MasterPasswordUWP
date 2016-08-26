using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using CryptSharp.Utility;

namespace MasterPasswordUWP.Algorithm
{
    // https://github.com/Lyndir/MasterPassword/blob/master/MasterPassword/Java/masterpassword-algorithm/src/main/java/com/lyndir/masterpassword/MasterKeyV0.java

    /**
     * bugs:
     * - V2: miscounted the byte-length fromInt multi-byte full names.
     * - V1: miscounted the byte-length fromInt multi-byte site names.
     * - V0: does math with chars whose signedness was platform-dependent.
     */
    public class MasterKeyV0 : MasterKey
    {
        protected int MP_N = 32768;
        protected int MP_r = 8;
        protected int MP_p = 2;
        protected int MP_dkLen = 64;
        protected int MP_intLen = 32;
        //protected Charset                      MP_charset   = Charsets.UTF_8;
        protected ByteOrder MP_byteOrder = ByteOrder.BigEndian;
        protected HashAlgorithm MP_hash = System.Security.Cryptography.SHA256.Create();
        protected HashAlgorithm MP_mac = new HMACSHA256();

        public override KeyVersion AlgorithmVersion => KeyVersion.V0;

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public MasterKeyV0(string fullName) : base(fullName)
        {
        }

        protected override byte[] DeriveKey(char[] masterPassword)
        {
            var fullNameBytes = Encoding.UTF8.GetBytes(FullName);
            var fullNameLengthBytes = BytesForInt(FullName.Length);

            var mpKeyScope = SiteVariantHelper.GetScope(SiteVariant.Password);
            var masterKeySalt = Concat(Encoding.UTF8.GetBytes(mpKeyScope), fullNameLengthBytes, fullNameBytes);

            return Scrypt(masterKeySalt, Encoding.UTF8.GetBytes(masterPassword));
        }

        protected byte[] Scrypt(byte[] masterKeySalt, byte[] mpBytes)
        {
            try
            {
                return SCrypt.ComputeDerivedKey(mpBytes, masterKeySalt, MP_N, MP_r, MP_p, null, MP_dkLen);
            }
            catch (Exception x)
            {
                //logger.bug(e); todo log
                return null;
            }
            finally
            {
                Fill(mpBytes, (byte)0);
            }
        }

        public override string Encode(string siteName, SiteType siteType, uint siteCounter, SiteVariant siteVariant, string siteContext = null)
        {
            if (siteCounter == 0)
            {
                siteCounter = (uint) (CurrentTimeMillis() / (300 * 1000)) * 300;
            }

            var siteScope = SiteVariantHelper.GetScope(siteVariant);
            var siteNameBytes = Encoding.UTF8.GetBytes(siteName);
            var siteNameLengthBytes = BytesForInt(siteName.Length);
            var siteCounterBytes = BytesForInt(siteCounter);
            var siteContextBytes = string.IsNullOrEmpty(siteContext) ? null : Encoding.UTF8.GetBytes(siteContext);
            var siteContextLengthBytes = BytesForInt(siteContextBytes?.Length ?? 0);

            var sitePasswordInfo = Concat(Encoding.UTF8.GetBytes(siteScope), siteNameLengthBytes, siteNameBytes, siteCounterBytes);
            if (siteContextBytes != null)
                sitePasswordInfo = Concat(sitePasswordInfo, siteContextLengthBytes, siteContextBytes);

            var sitePasswordSeedBytes = ComputeHmacOf(MasterKeyBytes, sitePasswordInfo);
            var sitePasswordSeed = new int[sitePasswordSeedBytes.Length];
            for (var i = 0; i < sitePasswordSeedBytes.Length; ++i)
            {
                var m = new MemoryStream(sizeof(int) / sizeof(byte));
                m.SetLength(sizeof(int) / sizeof(byte));
                Fill(m, (sbyte)sitePasswordSeedBytes[i] > 0 ? (byte)0x00 : (byte)0xFF);

                var w = new BinaryWriter(m);
                {
                    w.Seek(2, SeekOrigin.Begin);
                    w.Write(sitePasswordSeedBytes[i]);
                    w.Flush();
                    m.Position = 0;
                }

                // change byte order
                m = new MemoryStream(ChangeByteOrder(m.ToArray(), ByteOrder.BigEndian));

                using (var r = new BinaryReader(m))
                {
                    sitePasswordSeed[i] = r.ReadInt32() & 0xFFFF;
                }
            }

            var templateIndex = sitePasswordSeed[0];
            var template = SiteTypeHelper.GetTemplateAtRollingIndex(siteType, templateIndex);

            var password = new StringBuilder(template.TemplateString.Length);
            for (var i = 0; i < template.TemplateString.Length; ++i)
            {
                var characterIndex = sitePasswordSeed[i + 1];
                var characterClass = template.TemplateChars[i];
                var passwordCharacter = TemplateCharacterClassHelper.GetCharacterAtRollingIndex(characterClass, characterIndex);

                password.Append(passwordCharacter);
            }

            return password.ToString();
        }

        protected override byte[] BytesForInt(int number)
        {
            var r = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian && MP_byteOrder != ByteOrder.LittleEndian)
            {
                r = r.Reverse().ToArray();
            }
            return r;
        }

        protected override byte[] BytesForInt(uint number)
        {
            return BytesForInt((int)number);
        }

        protected override byte[] IdForBytes(byte[] bytes)
        {
            return MP_hash.ComputeHash(bytes);
        }
    }
}
