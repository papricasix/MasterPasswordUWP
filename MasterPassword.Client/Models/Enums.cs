using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using MasterPassword.Common.Models;

namespace MasterPasswordUWP.Models
{
    public enum SitesOrder
    {
        [LocalizedDisplayName("Enums_SitesOrder_Name")]
        Name,
        [LocalizedDisplayName("Enums_SitesOrder_Login")]
        Login,
        [LocalizedDisplayName("Enums_SitesOrder_LastUse")]
        LastUse
    }
}
