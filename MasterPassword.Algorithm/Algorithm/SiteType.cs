using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MasterPassword.Common.Models;

namespace MasterPasswordUWP.Algorithm
{
    // https://github.com/Lyndir/MasterPassword/blob/master/MasterPassword/Java/masterpassword-algorithm/src/main/java/com/lyndir/masterpassword/MPSiteType.java

    [AttributeUsage(AttributeTargets.Field)]
    public class SiteTypeAttribute : Attribute
    {
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string[] Options { get; set; }
        public string[] Templates { get; set; }
        public SiteTypeClass TypeClass { get; set; }
        public int TypeIndex { get; set; }
        public object[] TypeFeatures { get; set; }

        /*
         * there are two features in the original:
         * ExportContent( 1 << 10 ),
         * DevicePrivate( 1 << 11 );
         */
    }

    public enum SiteType
    {
        [LocalizedDisplayName("Enums_SiteType_Max")]
        [SiteType(ShortName = "Max", TypeIndex = 0x0, Templates = new [] { "anoxxxxxxxxxxxxxxxxx", "axxxxxxxxxxxxxxxxxno" })]
        GeneratedMaximum,

        [LocalizedDisplayName("Enums_SiteType_Long")]
        [SiteType(ShortName = "Long", TypeIndex = 0x1, Templates = new[] { "CvcvnoCvcvCvcv", "CvcvCvcvnoCvcv", "CvcvCvcvCvcvno", "CvccnoCvcvCvcv", "CvccCvcvnoCvcv", "CvccCvcvCvcvno", "CvcvnoCvccCvcv", "CvcvCvccnoCvcv", "CvcvCvccCvcvno", "CvcvnoCvcvCvcc", "CvcvCvcvnoCvcc", "CvcvCvcvCvccno", "CvccnoCvccCvcv", "CvccCvccnoCvcv", "CvccCvccCvcvno", "CvcvnoCvccCvcc", "CvcvCvccnoCvcc", "CvcvCvccCvccno", "CvccnoCvcvCvcc", "CvccCvcvnoCvcc", "CvccCvcvCvccno" })]
        GeneratedLong,
        

        [LocalizedDisplayName("Enums_SiteType_Medium")]
        [SiteType(ShortName = "Medium", TypeIndex = 0x2, Templates = new[] { "CvcnoCvc", "CvcCvcno" })]
        GeneratedMedium,
        

        [LocalizedDisplayName("Enums_SiteType_Basic")]
        [SiteType(ShortName = "Basic", TypeIndex = 0x3, Templates = new[] { "aaanaaan", "aannaaan", "aaannaaa" })]
        GeneratedBasic,
        

        [LocalizedDisplayName("Enums_SiteType_Short")]
        [SiteType(ShortName = "Short", TypeIndex = 0x4, Templates = new[] { "Cvcn" })]
        GeneratedShort,
        

        [LocalizedDisplayName("Enums_SiteType_PIN")]
        [SiteType(ShortName = "PIN", TypeIndex = 0x5, Templates = new[] { "nnnn" })]
        GeneratedPin,
        

        [LocalizedDisplayName("Enums_SiteType_Name")]
        [SiteType(ShortName = "Name", TypeIndex = 0xE, Templates = new[] { "cvccvcvcv" })]
        GeneratedName,
        

        [LocalizedDisplayName("Enums_SiteType_Phrase")]
        [SiteType(ShortName = "Phrase", TypeIndex = 0xF, Templates = new[] { "cvcc cvc cvccvcv cvc", "cvc cvccvcvcv cvcv", "cv cvccv cvc cvcvccv" })]
        GeneratedPhrase,

        /*[SiteType(ShortName = "Personal")]
        StoredPersonal,

        [SiteType(ShortName = "Device Private")]
        StoredDevicePrivate*/
    }

    public static class SiteTypeHelper
    {
        public static IEnumerable<string> GetSiteTypeNames()
        {
            return Enum.GetNames(typeof(SiteType));
        }

        public static string GetShortName(SiteType e)
        {
            return e.GetType().GetField(e.ToString()).GetCustomAttribute<SiteTypeAttribute>().ShortName;
        }

        public static SiteType ForName(string s)
        {
            return Enum.GetValues(typeof(SiteType)).Cast<SiteType>().Single(type => GetShortName(type) == s);
        }

        public static IEnumerable<string> GetTemplates(SiteType e)
        {
            return e.GetType().GetField(e.ToString()).GetCustomAttribute<SiteTypeAttribute>().Templates;
        }

        public static int GetTypeIndex(SiteType e)
        {
            return e.GetType().GetField(e.ToString()).GetCustomAttribute<SiteTypeAttribute>().TypeIndex;
        }

        public static SiteTypeClass GetTypeClass(SiteType e)
        {
            return e.GetType().GetField(e.ToString()).GetCustomAttribute<SiteTypeAttribute>().TypeClass;
        }

        public static int GetType(SiteType e)
        {
            var mask = GetTypeIndex(e) | SiteTypeClassHelper.GetMask(GetTypeClass(e));
            return mask;
        }

        public static IEnumerable<SiteType> ForMask(int mask)
        {
            var typeMask = mask & ~0xF;
            foreach (SiteType e in Enum.GetValues(typeof(SiteType)))
            {
                if (((GetType(e) & ~0xF) & typeMask) != 0)
                {
                    yield return e;
                }
            }
        }

        public static Template GetTemplateAtRollingIndex(SiteType e, int templateIndex)
        {
            var t = GetTemplates(e).ToArray();
            return new Template( t.ElementAt(templateIndex % t.Length) );
        }

        public static SiteType FromInt(int intValue)
        {
            foreach (SiteType e in Enum.GetValues(typeof(SiteType)))
            {
                if (GetType(e) == intValue)
                {
                    return e;
                }
            }
            throw new ArgumentException(nameof(intValue));
        }
    }
}
