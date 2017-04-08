using System;
using Windows.Storage;
using Template10.Common;
using Template10.Utils;
using Windows.UI.Xaml;
using MasterPassword.Client.Services.ImportExport;
using MasterPasswordUWP.Algorithm;
using MasterPasswordUWP.Models;
using MasterPasswordUWP.Services.DataSources;

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
                var value = _helper.Read(nameof(DataSourceFile), $@"{ApplicationData.Current.LocalFolder.Path}\sites.json");
                return value;
            }
            set { _helper.Write(nameof(DataSourceFile), value); }
        }

        public DataSourceType PreferredImportExportFileType
        {
            get { return _helper.Read(nameof(PreferredImportExportFileType), DataSourceType.Json); }
            set { _helper.Write(nameof(PreferredImportExportFileType), value); }
        }

        public string ExportDirectory
        {
            get
            {
                var value = _helper.Read(nameof(ExportDirectory), string.Empty);
                return value;
            }
            set { _helper.Write(nameof(ExportDirectory), value); }
        }

        public string PersistedUserName
        {
            get { return _helper.Read(nameof(PersistedUserName), string.Empty); }
            set { _helper.Write(nameof(PersistedUserName), value); }
        }

        public bool AllowCollectionOfTelemetryData
        {
            get { return _helper.Read(nameof(AllowCollectionOfTelemetryData), true); }
            set { _helper.Write(nameof(AllowCollectionOfTelemetryData), value); }
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

        public string AppLanguage
        {
            get { return _helper.Read(nameof(AppLanguage), string.Empty); }
            set { _helper.Write(nameof(AppLanguage), value); }
        }

        private bool _passwordVisibleDirty;

        public bool PopPasswordsVisibleChangedFlag()
        {
            try
            {
                return _passwordVisibleDirty;
            }
            finally
            {
                _passwordVisibleDirty = false;
            }
        }

        public bool PasswordsVisible
        {
            get
            {
                var visible = true;
                var value = _helper.Read<string>(nameof(PasswordsVisible), visible.ToString());
                return Boolean.TryParse(value, out visible) ? visible : true;
            }
            set
            {
                _passwordVisibleDirty = PasswordsVisible != value;
                _helper.Write(nameof(PasswordsVisible), value.ToString());
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
