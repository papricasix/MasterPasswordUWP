using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MasterPasswordUWP.Algorithm
{
    // https://github.com/Lyndir/MasterPassword/blob/master/MasterPassword/Java/masterpassword-algorithm/src/main/java/com/lyndir/masterpassword/MPTemplateCharacterClass.java

    [AttributeUsage(AttributeTargets.Field)]
    public class TemplateCharacterClassAttribute : Attribute
    {
        public char Identifier { get; set; }
        public string Characters { get; set; }
    }

    public enum TemplateCharacterClass
    {
        [TemplateCharacterClass(Identifier = 'V', Characters = "AEIOU")]
        UpperVowel,
        [TemplateCharacterClass(Identifier = 'C', Characters = "BCDFGHJKLMNPQRSTVWXYZ")]
        UpperConsonant,
        [TemplateCharacterClass(Identifier = 'v', Characters = "aeiou")]
        LowerVowel,
        [TemplateCharacterClass(Identifier = 'c', Characters = "bcdfghjklmnpqrstvwxyz")]
        LowerConsonant,
        [TemplateCharacterClass(Identifier = 'A', Characters = "AEIOUBCDFGHJKLMNPQRSTVWXYZ")]
        UpperAlphanumeric,
        [TemplateCharacterClass(Identifier = 'a', Characters = "AEIOUaeiouBCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz")]
        Alphanumeric,
        [TemplateCharacterClass(Identifier = 'n', Characters = "0123456789")]
        Numeric,
        [TemplateCharacterClass(Identifier = 'o', Characters = @"@&%?,=[]_:-+*$#!'^~;()/.")]
        Other,
        [TemplateCharacterClass(Identifier = 'x', Characters = @"AEIOUaeiouBCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz0123456789!@#$%^&*()")]
        Any,
        [TemplateCharacterClass(Identifier = ' ', Characters = " ")]
        Space
    }

    public static class TemplateCharacterClassHelper
    {
        public static char GetIdentifier(TemplateCharacterClass e)
        {
            return e.GetType().GetField(e.ToString()).GetCustomAttribute<TemplateCharacterClassAttribute>().Identifier;
        }

        public static string GetCharacters(TemplateCharacterClass e)
        {
            return e.GetType().GetField(e.ToString()).GetCustomAttribute<TemplateCharacterClassAttribute>().Characters;
        }

        public static char GetCharacterAtRollingIndex(TemplateCharacterClass e, int index)
        {
            var c = GetCharacters(e);
            return c[index % c.Length];
        }

        public static TemplateCharacterClass ForIdentifier(char identifier)
        {
            foreach (var e in (TemplateCharacterClass[]) Enum.GetValues(typeof(TemplateCharacterClass)))
            {
                if (identifier == GetIdentifier(e))
                {
                    return e;
                }
            }
            throw new InvalidOperationException($"unknown identifier {identifier}");
        }
    }
}
