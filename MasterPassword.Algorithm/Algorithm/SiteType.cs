using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    }

    public enum SiteType
    {
        [SiteType(ShortName = "Max", TypeIndex = 0x0, Templates = new [] { "anoxxxxxxxxxxxxxxxxx", "axxxxxxxxxxxxxxxxxno" })]
        GeneratedMaximum,

        [SiteType(ShortName = "Long", TypeIndex = 0x1, Templates = new[] { "CvcvnoCvcvCvcv", "CvcvCvcvnoCvcv", "CvcvCvcvCvcvno", "CvccnoCvcvCvcv", "CvccCvcvnoCvcv", "CvccCvcvCvcvno", "CvcvnoCvccCvcv", "CvcvCvccnoCvcv", "CvcvCvccCvcvno", "CvcvnoCvcvCvcc", "CvcvCvcvnoCvcc", "CvcvCvcvCvccno", "CvccnoCvccCvcv", "CvccCvccnoCvcv", "CvccCvccCvcvno", "CvcvnoCvccCvcc", "CvcvCvccnoCvcc", "CvcvCvccCvccno", "CvccnoCvcvCvcc", "CvccCvcvnoCvcc", "CvccCvcvCvccno" })]
        GeneratedLong,

        [SiteType(ShortName = "Medium", TypeIndex = 0x2, Templates = new[] { "CvcnoCvc", "CvcCvcno" })]
        GeneratedMedium,

        [SiteType(ShortName = "Basic", TypeIndex = 0x3, Templates = new[] { "aaanaaan", "aannaaan", "aaannaaa" })]
        GeneratedBasic,

        [SiteType(ShortName = "Short", TypeIndex = 0x4, Templates = new[] { "Cvcn" })]
        GeneratedShort,

        [SiteType(ShortName = "PIN", TypeIndex = 0x5, Templates = new[] { "nnnn" })]
        GeneratedPin,

        [SiteType(ShortName = "Name", TypeIndex = 0xE, Templates = new[] { "cvccvcvcv" })]
        GeneratedName,

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
            foreach (var n in Enum.GetValues(typeof(SiteType)))
            {
                
            }
            throw new NotImplementedException();
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
    }
}
