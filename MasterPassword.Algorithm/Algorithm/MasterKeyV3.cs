using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MasterPasswordUWP.Algorithm
{
    public class MasterKeyV3 : MasterKeyV2
    {
        public override KeyVersion AlgorithmVersion => KeyVersion.V3;

        public MasterKeyV3(string fullName) : base(fullName)
        {
        }

        protected override byte[] DeriveKey(char[] masterPassword)
        {
            var fullNameBytes = Encoding.UTF8.GetBytes(FullName);
            var fullNameLengthBytes = BytesForInt(fullNameBytes.Length);

            var mpKeyScope = SiteVariantHelper.GetScope(SiteVariant.Password);
            var masterKeySalt = Concat(Encoding.UTF8.GetBytes(mpKeyScope), fullNameLengthBytes, fullNameBytes);

            return Scrypt(masterKeySalt, Encoding.UTF8.GetBytes(masterPassword));
        }
    }
}
