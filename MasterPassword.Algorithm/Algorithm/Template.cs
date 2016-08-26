using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterPasswordUWP.Algorithm
{
    // https://github.com/Lyndir/MasterPassword/blob/master/MasterPassword/Java/masterpassword-algorithm/src/main/java/com/lyndir/masterpassword/MPTemplate.java

    public class Template
    {
        public string TemplateString { get; set; }
        public TemplateCharacterClass[] TemplateChars { get; set; }

        public Template(string templStr)
        {
            TemplateChars = templStr.Select(TemplateCharacterClassHelper.ForIdentifier).ToArray();
            TemplateString = templStr;
        }
    }

    public static class TemplateHelper
    {
        
    }
}
