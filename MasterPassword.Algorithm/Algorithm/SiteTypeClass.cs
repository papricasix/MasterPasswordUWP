using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MasterPasswordUWP.Algorithm
{
    // https://github.com/Lyndir/MasterPassword/blob/master/MasterPassword/Java/masterpassword-algorithm/src/main/java/com/lyndir/masterpassword/MPSiteTypeClass.java

    [AttributeUsage(AttributeTargets.Field)]
    public class SiteTypeClassAttribute : Attribute
    {
        public int Mask { get; set; }
    }

    public enum SiteTypeClass
    {
        [SiteTypeClass(Mask = 1 << 4)]
        Generated,
        [SiteTypeClass(Mask = 1 << 5)]
        Stored
    }

    public static class SiteTypeClassHelper
    {
        public static int GetMask(SiteTypeClass e)
        {
            return e.GetType().GetField(e.ToString()).GetCustomAttribute<SiteTypeClassAttribute>().Mask;
        }
    }
}
