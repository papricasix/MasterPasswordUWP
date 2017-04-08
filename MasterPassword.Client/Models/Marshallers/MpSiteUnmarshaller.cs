using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MasterPasswordUWP.Algorithm;
using MasterPasswordUWP.Models;

namespace MasterPassword.Client.Models.Marshallers
{
    public class MpSiteUnmarshaller : ICustomSiteUnmarshaller
    {
        private Regex[] _unmarshallFormats =
        {
            new Regex("^([^ ]+) +(\\d+) +(\\d+)(:\\d+)? +([^\t]+)\t(.*)"), 
            new Regex("^([^ ]+) +(\\d+) +(\\d+)(:\\d+)?(:\\d+)? +([^\t]*)\t *([^\t]+)\t(.*)"), 
        };
        private Regex _headerFormat = new Regex("^#\\s*([^:]+): (.*)");

        public IEnumerable<ISite> UnmarshallSites(string data)
        {
            var stream = StringToStreamReader(data);
            string line;
            while ((line = stream.ReadLine()) != null)
            {
                var site = UnmarshallSite(line);
                if (site != null)
                {
                    yield return site;
                }
            }
        }

        private static StreamReader StringToStreamReader(string data)
        {
            return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(data)));
        }

        /*private Header UnmarshallHeader(string line)
        {
            
        }*/

        private ISite UnmarshallSite(string line)
        {
            if (line?.StartsWith("#") ?? true)
            {
                return null;
            }

            var match = _unmarshallFormats[1].Match(line);
            if (match.Success)
            {
                return new Site
                {
                    AlgorithmVersion = KeyVersionHelper.FromInt(Convert.ToInt32(match.Groups[4].Value.Replace(":", ""))),
                    LastUsed = Convert.ToDateTime(match.Groups[1].Value).ToFileTimeUtc(),
                    SiteName = match.Groups[7].Value,
                    PasswordType = SiteTypeHelper.FromInt(Convert.ToInt32(match.Groups[3].Value)),
                    SiteCounter = Convert.ToInt32(match.Groups[5].Value.Replace(":", "")),
                    UserName = match.Groups[6].Value,
                };
            }

            return null;
        }
    }
}
