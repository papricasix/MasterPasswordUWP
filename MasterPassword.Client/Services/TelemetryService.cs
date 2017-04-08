using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Services.Store.Engagement;

namespace MasterPasswordUWP.Services
{
    internal interface ITelemetryService
    {
        void LogMessage(object callerObj, [CallerMemberName]string callerName = null, string message = null);
    }

    internal class TelemetryService : ITelemetryService
    {
        private SettingsServices.SettingsService _settingsProvider;

        public bool TelemetryEnabled => /*_settingsProvider.AllowCollectionOfTelemetryData*/false;

        public TelemetryService(SettingsServices.SettingsService settings)
        {
            _settingsProvider = settings;
        }

        public void LogMessage(object callerObj, [CallerMemberName]string callerName = null, string message = null)
        {
            if (callerObj != null && TelemetryEnabled)
            {
                StoreServicesCustomEventLogger.GetDefault().Log( $"{callerObj.GetType().Name} {callerName ?? string.Empty} {message ?? string.Empty}".TrimEnd() );
            }
        }
    }
}
