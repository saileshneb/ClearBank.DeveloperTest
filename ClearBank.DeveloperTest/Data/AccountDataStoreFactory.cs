using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClearBank.DeveloperTest.Data
{
    public class AccountDataStoreFactory : IAccountDataStoreFactory
    {
        private const string DataStoreTypeKey = "DataStoreType";
        private const string BackupStoreValue = "Backup";

        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountDataStoreFactory> _logger;

        public AccountDataStoreFactory(IConfiguration configuration, ILogger<AccountDataStoreFactory> logger)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(logger);
            _configuration = configuration;
            _logger = logger;
        }

        public IAccountDataStore Create()
        {
            var dataStoreType = _configuration[DataStoreTypeKey];

            if (string.IsNullOrWhiteSpace(dataStoreType))
            {
                _logger.LogWarning("DataStoreType not configured, using default implementation");
            }

            /*
             Instead of newing here, we could use a DI container to resolve the correct 
             implementation based on configuration. Something like the Validators. 
             */
            return dataStoreType == BackupStoreValue
                ? new BackupAccountDataStore()
                : new AccountDataStore();
        }
    }
}
