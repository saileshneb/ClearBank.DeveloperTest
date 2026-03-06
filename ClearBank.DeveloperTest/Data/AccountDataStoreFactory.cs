using System;
using Microsoft.Extensions.Configuration;

namespace ClearBank.DeveloperTest.Data
{
    public class AccountDataStoreFactory : IAccountDataStoreFactory
    {
        private const string DataStoreTypeKey = "DataStoreType";
        private const string BackupStoreValue = "Backup";

        private readonly IConfiguration _configuration;

        public AccountDataStoreFactory(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _configuration = configuration;
        }

        public IAccountDataStore Create()
        {
            var dataStoreType = _configuration[DataStoreTypeKey];

            if (string.IsNullOrWhiteSpace(dataStoreType))
            {
                //log warning about missing configuration and defaulting to AccountDataStore
            }

            return dataStoreType == BackupStoreValue
                ? new BackupAccountDataStore()
                : new AccountDataStore();
        }
    }
}
