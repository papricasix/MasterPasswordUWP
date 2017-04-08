using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterPasswordUWP.Models;

namespace MasterPassword.Client.Models.Marshallers
{
    public interface ICustomSiteMarshaller
    {
        string MarshallSites(string userName, IEnumerable<ISite> sites);
    }

    public interface ICustomSiteUnmarshaller
    {
        IEnumerable<ISite> UnmarshallSites(string data);
    }
}
