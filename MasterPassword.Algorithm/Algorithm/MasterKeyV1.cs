using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterPasswordUWP.Algorithm
{
    public class MasterKeyV1 : MasterKeyV0
    {
        public override KeyVersion AlgorithmVersion => KeyVersion.V1;

        public MasterKeyV1(string fullName) : base(fullName)
        {
        }

        public override string Encode(string siteName, SiteType siteType, uint siteCounter, SiteVariant siteVariant, string siteContext = null)
        {
            if (siteCounter == 0)
            {
                siteCounter = (uint)(CurrentTimeMillis() / (300 * 1000)) * 300;
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

            var sitePasswordSeed = ComputeHmacOf(MasterKeyBytes, sitePasswordInfo);
            var templateIndex = sitePasswordSeed[0] & 0xFF; // Mask the integer's sign.
            var template = SiteTypeHelper.GetTemplateAtRollingIndex(siteType, templateIndex);

            var password = new StringBuilder(template.TemplateString.Length);
            for (var i = 0; i < template.TemplateString.Length; ++i)
            {
                var characterIndex = sitePasswordSeed[i + 1] & 0xFF; // Mask the integer's sign.
                var characterClass = template.TemplateChars[i];
                var passwordCharacter = TemplateCharacterClassHelper.GetCharacterAtRollingIndex(characterClass, characterIndex);

                password.Append(passwordCharacter);
            }

            return password.ToString();
        }
    }
}
