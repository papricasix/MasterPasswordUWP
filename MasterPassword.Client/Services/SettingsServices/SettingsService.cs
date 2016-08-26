using System;
using Windows.Storage;
using Template10.Common;
using Template10.Utils;
using Windows.UI.Xaml;
using MasterPasswordUWP.Algorithm;
using MasterPasswordUWP.Models;

namespace MasterPasswordUWP.Services.SettingsServices
{
    public class SettingsService
    {
        //public static SettingsService Instance { get; } = new SettingsService();
        Template10.Services.SettingsService.ISettingsHelper _helper;

        public SettingsService()
        {
            _helper = new Template10.Services.SettingsService.SettingsHelper();
        }

        public string DataSourceFile
        {
            get
            {
                var value = _helper.Read(nameof(DataSourceFile),
                    $@"{ApplicationData.Current.LocalFolder.Path}\sites.json");
                return value;
            }
            set { _helper.Write(nameof(DataSourceFile), value); }
        }

        public string PersistedUserName
        {
            get { return _helper.Read(nameof(PersistedUserName), string.Empty); }
            set { _helper.Write(nameof(PersistedUserName), value); }
        }

        //public string MasterPassword { get; set; }
        public MasterKey MasterKey { get; set; }

        public ApplicationTheme AppTheme
        {
            get
            {
                var theme = ApplicationTheme.Light;
                var value = _helper.Read<string>(nameof(AppTheme), theme.ToString());
                return Enum.TryParse<ApplicationTheme>(value, out theme) ? theme : ApplicationTheme.Dark;
            }
            set
            {
                _helper.Write(nameof(AppTheme), value.ToString());
                (Window.Current.Content as FrameworkElement).RequestedTheme = value.ToElementTheme();
                Views.Shell.HamburgerMenu.RefreshStyles(value);
            }
        }

        public TimeSpan CacheMaxDuration
        {
            get { return _helper.Read<TimeSpan>(nameof(CacheMaxDuration), TimeSpan.FromDays(2)); }
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }

        public SitesOrder LastSortBy
        {
            get { return _helper.Read(nameof(LastSortBy), SitesOrder.Name); }
            set { _helper.Write(nameof(LastSortBy), value); }
        }
    }
}
