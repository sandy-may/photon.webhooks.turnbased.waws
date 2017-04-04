using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Photon.Turnbased.Config;
using Photon.Webhooks.Turnbased.DataAccess;

namespace Photon.Turnbased.DataAccess
{
    public class DataSources
    {
        public static IDataAccess DataAccess;
        public static CloudStorageAccount CloudStorageAccount;
        private readonly AppSettings _appSettings;
        private readonly ConnectionStrings _connectionStrings;

        public DataSources(IOptions<AppSettings> dataAccessor, IOptions<ConnectionStrings> connectionStrings)
        {
            _appSettings = dataAccessor.Value;
            _connectionStrings = connectionStrings.Value;
        }

        private void CreatDataAccessor()
        {
            if (_appSettings.DataSource.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            {
                CloudStorageAccount = CloudStorageAccount.Parse(_connectionStrings.AzureBlobConnectionString);
                DataAccess = new Azure(_connectionStrings.AzureBlobConnectionString);
            }
            else if (_appSettings.DataSource.Equals("Redis", StringComparison.OrdinalIgnoreCase))
            {
                
            }
        }
        
    }
}
