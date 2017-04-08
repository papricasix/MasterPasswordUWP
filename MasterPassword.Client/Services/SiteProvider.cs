using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using MasterPasswordUWP.Models;
using MasterPasswordUWP.Services.DataSources;
using MasterPasswordUWP.Services.SettingsServices;
using Template10.Controls;

namespace MasterPasswordUWP.Services
{
    public interface ISiteProvider
    {
        IEnumerable<ISite> Sites { get; set; }
    }

    public interface ISitePersistor
    {
        void Persist(IEnumerable<ISite> sites);
    }

    public interface ISiteImporterExporter
    {
        Task Import(DataSourceType type, StorageFile file);

        Task Export(DataSourceType type, StorageFile file);
    }

    static class ProviderHelper
    {
        public static ISiteDataSource GetDataSourceFor(IEnumerable<ISiteDataSource> sources, DataSourceType type)
        {
            foreach ( var e in sources )
            {
                if ( e.DataSourceType == type )
                {
                    return e;
                }
            }
            throw new NullReferenceException( $"{nameof(sources)} is missing for {nameof(type)} == {type}" );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SiteProvider : ISiteProvider
    {
        private readonly IEnumerable<ISiteDataSource> _dataSources;
        private readonly SettingsService _settings;

        private IEnumerable<ISite> _sitesCache;

        public IEnumerable<ISite> Sites
        {
            get { return GetCached(Read); }
            set { _sitesCache = value;  }
        }

        public SiteProvider(IEnumerable<ISiteDataSource> dataSources, SettingsService settings)
        {
            _dataSources = dataSources;
            _settings = settings;
        }

        private IEnumerable<ISite> GetCached(Func<IEnumerable<ISite>> retrieverFunc)
        {
            return _sitesCache ?? (_sitesCache = retrieverFunc?.Invoke());
        }

        private IEnumerable<ISite> Read()
        {
            // default type is Json right now. no need to pass this parameter to any consumer
            var dataSource = ProviderHelper.GetDataSourceFor(_dataSources, DataSourceType.Json);
            if ( !File.Exists(_settings.DataSourceFile) )
            {
                File.WriteAllText( _settings.DataSourceFile, dataSource.CreateNew() as string );
            }
            return dataSource.DeserializeFrom( File.ReadAllText( _settings.DataSourceFile ) );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SitePersistor : ISitePersistor, ISiteImporterExporter
    {
        private readonly ISiteProvider _siteProvider;
        private readonly IEnumerable<ISiteDataSource> _dataSources;
        private readonly SettingsService _settings;

        public SitePersistor(IEnumerable<ISiteDataSource> dataSources, ISiteProvider siteProvider, SettingsService settings)
        {
            _siteProvider = siteProvider;
            _dataSources = dataSources;
            _settings = settings;
        }

        public async Task Import(DataSourceType type, StorageFile file)
        {
            if (file == null)
            {
                return;
            }
            var stream = await file.OpenStreamForReadAsync();
            using (var e = new StreamReader(stream))
            {
                var sitesToImport = ProviderHelper.GetDataSourceFor(_dataSources, type).DeserializeFrom(e.ReadToEnd());
                await ProviderHelper.GetDataSourceFor(_dataSources, DataSourceType.Json).Serialize(sitesToImport, _settings.DataSourceFile);
            }

            // clear cache, if applicable
            _siteProvider.Sites = null;
        }
        public async Task Export(DataSourceType type, StorageFile file)
        {
            if ( file == null )
            {
                return;
            }
            await ProviderHelper.GetDataSourceFor(_dataSources, type).Serialize(_siteProvider.Sites, file);
        }

        public async void Persist(IEnumerable<ISite> sites)
        {
            await ProviderHelper.GetDataSourceFor(_dataSources, DataSourceType.Json).Serialize(sites, _settings.DataSourceFile);
            // clear cache, if applicable
            _siteProvider.Sites = null;
        }
    }
}
