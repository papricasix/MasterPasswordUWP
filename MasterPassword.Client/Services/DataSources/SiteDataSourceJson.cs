using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using MasterPasswordUWP.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MasterPasswordUWP.Services.DataSources
{
    public class SiteDataSourceJson : ISiteDataSource
    {
        public DataSourceType DataSourceType { get; set; }

        public SiteDataSourceJson(DataSourceType dataSourceType)
        {
            DataSourceType = dataSourceType;
        }

        public object CreateNew()
        {
            return JsonConvert.SerializeObject(new List<ISite>());
        }

        public IEnumerable<ISite> DeserializeFrom(object source)
        {
            try
            {
                return JsonConvert.DeserializeObject<IEnumerable<Site>>(source as string, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (JsonReaderException)
            {
                // todo logging
                return new List<ISite>();
            }
        }

        public async Task<bool> Serialize(IEnumerable<ISite> sites, object target)
        {
            var content = JsonConvert.SerializeObject(sites, new JsonSerializerSettings { Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver()});
            if (target is string)
            {
                target = await StorageFile.GetFileFromPathAsync(target as string);
            }
            if (target is StorageFile)
            {
                var store = target as StorageFile;
                using (var io = await store.OpenTransactedWriteAsync(StorageOpenOptions.AllowOnlyReaders))
                {
                    using (var w = new StreamWriter(io.Stream.AsStreamForWrite()))
                    {
                        w.BaseStream.SetLength(0);
                        await w.WriteAsync(content);
                    }
                    await io.CommitAsync();
                }

                return true;
            }
            return false;
        }
    }
}
