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
        void Import(StorageFile file);

        void Export(StorageFile file);
    }

    /// <summary>
    /// 
    /// </summary>
    public class SiteProvider : ISiteProvider
    {
        private readonly ISiteDataSource _dataSource;
        private readonly SettingsService _settings;

        private IEnumerable<ISite> _sitesCache;

        public IEnumerable<ISite> Sites
        {
            get { return GetCached(Read); }
            set { _sitesCache = value;  }
        }

        public SiteProvider(ISiteDataSource dataSource, SettingsService settings)
        {
            _dataSource = dataSource;
            _settings = settings;
        }

        private IEnumerable<ISite> GetCached(Func<IEnumerable<ISite>> retrieverFunc)
        {
            return _sitesCache ?? (_sitesCache = retrieverFunc?.Invoke());
        }

        private IEnumerable<ISite> Read()
        {
            if ( !File.Exists( _settings.DataSourceFile ) )
            {
                File.WriteAllText( _settings.DataSourceFile, _dataSource.CreateNew() as string );
            }
            return _dataSource.DeserializeFrom( File.ReadAllText( _settings.DataSourceFile ) );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SitePersistor : ISitePersistor, ISiteImporterExporter
    {
        private readonly ISiteProvider _siteProvider;
        private readonly ISiteDataSource _dataSource;
        private readonly SettingsService _settings;

        public SitePersistor(ISiteDataSource dataSource, ISiteProvider siteProvider, SettingsService settings)
        {
            _siteProvider = siteProvider;
            _dataSource = dataSource;
            _settings = settings;
        }

        public async void Import(StorageFile file)
        {
            if (file == null)
            {
                return;
            }
            var stream = await file.OpenStreamForReadAsync();
            using (var e = new StreamReader(stream))
            {
                var sitesToImport = _dataSource.DeserializeFrom(e.ReadToEnd());
                await _dataSource.Serialize(sitesToImport, _settings.DataSourceFile);
            }
            // clear cache, if applicable
            _siteProvider.Sites = null;
        }

        public async void Export(StorageFile file)
        {
            if ( file == null )
            {
                return;
            }
            await _dataSource.Serialize( _siteProvider.Sites, file );
        }

        public async void Persist(IEnumerable<ISite> sites)
        {
            await _dataSource.Serialize(sites, _settings.DataSourceFile);
            // clear cache, if applicable
            _siteProvider.Sites = null;
        }
    }
}
