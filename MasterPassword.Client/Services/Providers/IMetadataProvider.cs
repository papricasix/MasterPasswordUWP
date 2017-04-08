using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterPassword.Client.Services.Providers
{
    public interface IMetadataProvider
    {
        string UserName { get; }
    }
}
