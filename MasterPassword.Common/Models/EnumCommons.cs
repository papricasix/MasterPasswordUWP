using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace MasterPassword.Common.Models
{

    public class LocalizedDisplayNameAttribute : Attribute
    {
        private string _displayNameResource;

        public string DisplayName => ResourceLoader.GetForCurrentView().GetString(_displayNameResource);

        public LocalizedDisplayNameAttribute(string displayNameResource)
        {
            _displayNameResource = displayNameResource;
        }
    }

    public static class EnumExtension
    {
        public static string ToHumanReadableString(this Enum e)
        {
            var attr = e.GetType().GetMember(e.ToString()).First().GetCustomAttribute(typeof(LocalizedDisplayNameAttribute)) as LocalizedDisplayNameAttribute;
            return attr?.DisplayName ?? e.ToString();
        }


        public static Enum FromHumanReadableString(this string humanReadableString, Type baseEnumType, Enum defaultValue = default(Enum))
        {
            foreach (Enum e in Enum.GetValues(baseEnumType))
            {
                if (e.ToHumanReadableString() == humanReadableString)
                {
                    return e;
                }
            }
            return defaultValue;
        }
    }
}
