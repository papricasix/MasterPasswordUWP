using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Autofac;
using MasterPasswordUWP;
using MasterPasswordUWP.Services;
using MasterPasswordUWP.Services.DataSources;

namespace MasterPassword.Client.Services.ImportExport
{
    internal class SiteImportExportService : ISiteImportExportService
    {
        private readonly ISiteImporterExporter _importer;
        private readonly ISiteProvider _siteProvider;
        private readonly ITelemetryService _telemetry;

        public SiteImportExportService(ISiteImporterExporter siteImporterExporter, ISiteProvider siteProvider, ITelemetryService telemetry)
        {
            _importer = siteImporterExporter;
            _siteProvider = siteProvider;
            _telemetry = telemetry;
        }

        public async Task<bool> ImportSitesFromUserInputSource(bool bWarnAboutOverwrite, DataSourceType preferredType = DataSourceType.Json)
        {
            if (bWarnAboutOverwrite && _siteProvider.Sites.Any() && await DisplayImportWarnDialog() == ContentDialogResult.Primary)
            {
                return false;
            }

            var picker = new FileOpenPicker { ViewMode = PickerViewMode.List, SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            if (preferredType == DataSourceType.Json)
            {
                picker.FileTypeFilter.Add($".{TypeToExtension(DataSourceType.Json)}");
                picker.FileTypeFilter.Add($".{TypeToExtension(DataSourceType.MpSites)}");
            }
            if (preferredType == DataSourceType.MpSites)
            {
                picker.FileTypeFilter.Add($".{TypeToExtension(DataSourceType.MpSites)}");
                picker.FileTypeFilter.Add($".{TypeToExtension(DataSourceType.Json)}");
            }

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return false;
            }

            preferredType = ExtensionToType(file.Name);
            await _importer.Import(preferredType, file);

            _telemetry.LogMessage(this);

            return true;
        }

        public async Task<bool> ExportSitesToUserInputTarget(DataSourceType preferredType = DataSourceType.Json)
        {
            var picker = new FileSavePicker { SuggestedFileName = $"mpw-export.{TypeToExtension(preferredType)}", SuggestedStartLocation = PickerLocationId.DocumentsLibrary };
            if (preferredType == DataSourceType.Json)
            {
                picker.FileTypeChoices.Add("JSON", new List<string> { $".{TypeToExtension(DataSourceType.Json)}" });
                picker.FileTypeChoices.Add("MPSites", new List<string> { $".{TypeToExtension(DataSourceType.MpSites)}" });
            }
            if (preferredType == DataSourceType.MpSites)
            {
                picker.FileTypeChoices.Add("MPSites", new List<string> { $".{TypeToExtension(DataSourceType.MpSites)}" });
                picker.FileTypeChoices.Add("JSON", new List<string> { $".{TypeToExtension(DataSourceType.Json)}" });
            }

            var file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                return false;
            }

            preferredType = ExtensionToType(file.Name);
            await _importer.Export(preferredType, file);

            _telemetry.LogMessage(this, message: $"{preferredType}");

            return true;
        }

        private static string TypeToExtension(DataSourceType type)
        {
            switch (type)
            {
                case DataSourceType.Json:
                    return "json";
                case DataSourceType.MpSites:
                    return "mpsites";
                default:
                    return "json";
            }
        }

        private static DataSourceType ExtensionToType(string s)
        {
            if (s?.EndsWith(TypeToExtension(DataSourceType.Json)) ?? false)
            {
                return DataSourceType.Json;
            }
            if (s?.EndsWith(TypeToExtension(DataSourceType.MpSites)) ?? false)
            {
                return DataSourceType.MpSites;
            }
            return DataSourceType.Json;
        }

        private static async Task<ContentDialogResult> DisplayImportWarnDialog()
        {
            var rl = ResourceLoader.GetForCurrentView();
            var dialog = new ContentDialog
            {
                Title = rl.GetString("Messages_ImportSites_Title"),
                Content = rl.GetString("Messages_ImportSites_Content"),
                PrimaryButtonText = rl.GetString("Messages_ImportSites_PrimaryButtonText"),
                PrimaryButtonCommandParameter = true,
                SecondaryButtonText = rl.GetString("Messages_ImportSites_SecondaryButtonText"),
                SecondaryButtonCommandParameter = false,
            };

            return await dialog.ShowAsync();
        }
    }
}