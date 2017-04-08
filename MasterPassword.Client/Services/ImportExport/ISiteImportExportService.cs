using System.Threading.Tasks;
using MasterPasswordUWP.Services.DataSources;

namespace MasterPassword.Client.Services.ImportExport
{
    internal interface ISiteImportExportService
    {
        Task<bool> ImportSitesFromUserInputSource(bool bWarnAboutOverwrite, DataSourceType preferredType = DataSourceType.Json);

        Task<bool> ExportSitesToUserInputTarget(DataSourceType preferredType = DataSourceType.Json);
    }
}
