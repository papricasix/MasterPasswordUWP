using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterPasswordUWP.Models;

namespace MasterPasswordUWP.Services.DataSources
{
    public interface ISiteDataSource
    {
        object CreateNew();
        IEnumerable<ISite> DeserializeFrom(object source);
        Task<bool> Serialize(IEnumerable<ISite> sites, object target);
    }
}
