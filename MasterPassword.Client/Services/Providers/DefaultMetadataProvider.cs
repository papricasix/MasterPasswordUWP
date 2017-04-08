using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterPasswordUWP.Services.SettingsServices;

namespace MasterPassword.Client.Services.Providers
{
    public class DefaultMetadataProvider : IMetadataProvider
    {
        private SettingsService _settingsService;

        public string UserName => _settingsService.PersistedUserName;

        public DefaultMetadataProvider(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }
    }
}
