using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterPasswordUWP.Models;

namespace MasterPasswordUWP.Services.DataSources
{
    public enum DataSourceType
    {
        Json,

        MpSites
    }

    public interface ISiteDataSource
    {
        DataSourceType DataSourceType { get; }

        object CreateNew();
        IEnumerable<ISite> DeserializeFrom(object source);
        Task<bool> Serialize(IEnumerable<ISite> sites, object target);
    }
}
