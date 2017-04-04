using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Logging;
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

        public DataSources(IOptions<AppSettings> dataAccessor, IOptions<ConnectionStrings> connectionStrings, ILogger<Azure> logger)
        {
            _appSettings = dataAccessor.Value;
            _connectionStrings = connectionStrings.Value;
            CreatDataAccessor(logger);
        }

        private void CreatDataAccessor(ILogger<Azure> logger)
        {
            if (_appSettings.DataSource.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            {
                CloudStorageAccount = CloudStorageAccount.Parse(_connectionStrings.AzureBlobConnectionString);
                DataAccess = new Azure(logger, _connectionStrings.AzureBlobConnectionString);
            }
            else if (_appSettings.DataSource.Equals("Redis", StringComparison.OrdinalIgnoreCase))
            {
                //TODO: Setup up the redis local cache here   
            }
        }
        
    }
}
