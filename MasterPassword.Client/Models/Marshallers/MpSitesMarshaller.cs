using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;
using MasterPasswordUWP.Algorithm;
using MasterPasswordUWP.Models;
using MasterPasswordUWP.Services;

namespace MasterPassword.Client.Models.Marshallers
{
    /*
# Master Password site export
#     Export of site names and stored passwords (unless device-private) encrypted with the master key.
# 
##
# User Name: user name
# Avatar: 0
# Key ID: [some SHAx-Hash]
# Date: 2016-12-05T22:19:39Z
# Version: 2222222222222.4.60
# Format: 1
# Passwords: PROTECTED
##
#
#               Last     Times  Password                      Login	                     Site	Site
#               used      used      type                       name	                     name	password
2016-12-04T21:28:52Z         3    16:3:1                           	             sitename.com	
2016-12-13T08:40:33Z         1    31:3:1                     phrase	                        1	
2016-12-13T08:41:11Z         1    30:3:1                       name	                        2	
2016-12-13T08:41:35Z         1    16:3:1                    maximum	                        3	
2016-12-13T08:41:53Z         1    17:3:1                       long	                        4	
2016-12-13T08:42:12Z         1    18:3:1                     medium	                        5	
2016-12-13T08:42:27Z         1    20:3:1                      basic	                        6	
2016-12-13T08:42:42Z         1    19:3:1                      short	                        7	
2016-12-13T08:42:59Z         1    21:3:1                        pin	                        8	
     * */

    // base: https://github.com/Lyndir/MasterPassword/blob/73421b32998d1bc436d3d99e8b3eabd404a8b5a0/MasterPassword/Java/masterpassword-model/src/main/java/com/lyndir/masterpassword/model/MPSiteMarshaller.java

    public class MpSiteMarshaller : ICustomSiteMarshaller
    {
        public string MarshallSites(string userName, IEnumerable<ISite> sites)
        {
            var bldr = new StringBuilder();

            MarshallHeader(bldr, userName);
            foreach (var e in sites)
            {
                MarshallSite(bldr, e);
            }
            return bldr.ToString();
        }

        private void MarshallHeader(StringBuilder target, string userName)
        {
            target.Append("# Master Password site export\n");
            target.Append("#     ").Append("Export of site names and stored passwords (unless device-private) encrypted with the master key.").Append('\n');
            target.Append("# \n");
            target.Append("##\n");
            target.Append("# Format: 1\n");
            target.Append("# Date: ").Append(DateTime.UtcNow.ToString("o")).Append('\n');
            target.Append("# User Name: ").Append(userName).Append('\n');
            target.Append("# Full Name: ").Append(userName).Append('\n');
            target.Append("# Avatar: ").Append("0").Append('\n');
            //target.Append("# Key ID: ").Append(/*user.exportKeyID()*/"").Append('\n');
            target.Append("# Version: ").Append(/*MasterKey.Version.CURRENT.toBundleVersion()*/"0").Append('\n');
            target.Append("# Algorithm: ").Append(/*MasterKey.Version.CURRENT.toInt()*/"0").Append('\n');
            target.Append("# Default Type: ").Append($"{SiteTypeHelper.GetType(SiteType.GeneratedMaximum)}").Append('\n');
            target.Append("# Passwords: ").Append(/*this.contentMode.name()*/"PROTECTED").Append('\n');
            target.Append("##\n");
            target.Append("#\n");
            target.Append("#               Last     Times  Password                      Login\t                     Site\tSite\n");
            target.Append("#               used      used      type                       name\t                     name\tpassword\n");
        }

        private void MarshallSite(StringBuilder target, ISite site)
        {
            var passParams = ConvertToPaddedString($"{SiteTypeHelper.GetType(site.PasswordType)}:{(int)site.AlgorithmVersion}:{site.SiteCounter}", 8);

            var loginName = ConvertToPaddedString(site.UserName); // aka. LoginName
            var siteName = ConvertToPaddedString(site.SiteName);

            var exportLine = $"{DateTime.FromFileTime(site.LastUsed).ToUniversalTime():yyyy-MM-ddTHH\\:mm\\:ssZ}  {ConvertToPaddedString(0, 8)}  {passParams}  {loginName}\t{siteName}\t{string.Empty}"; // password
            target.Append(exportLine).Append('\n');
        }

        private static string ConvertToPaddedString(object o, int minLen = 25)
        {
            var s = o.ToString();
            s = s.PadLeft(minLen);
            return s;
        }
    }
}
