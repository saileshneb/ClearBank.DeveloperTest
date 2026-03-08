using ClearBank.DeveloperTest.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class AccountDataStoreFactoryTests
    {
        private readonly Mock<IConfiguration> _configMock = new Mock<IConfiguration>();
        private readonly Mock<ILogger<AccountDataStoreFactory>> _loggerMock = new Mock<ILogger<AccountDataStoreFactory>>();
       
        [Fact]
        public void Create_ConfigMissing_ReturnsAccountDataStore()
        {
            _configMock.Setup(c => c["DataStoreType"]).Returns((string)null);
            var sut = new AccountDataStoreFactory(_configMock.Object, _loggerMock.Object);

            var result = sut.Create();

            Assert.IsType<AccountDataStore>(result);
        }

        [Fact]
        public void Create_ConfigUnrecognised_ReturnsAccountDataStore()
        {
            _configMock.Setup(c => c["DataStoreType"]).Returns("Primary");
            var sut = new AccountDataStoreFactory(_configMock.Object, _loggerMock.Object);

            var result = sut.Create();

            Assert.IsType<AccountDataStore>(result);
        }

        [Fact]
        public void Create_ConfigBackup_ReturnsBackupAccountDataStore()
        {
            _configMock.Setup(c => c["DataStoreType"]).Returns("Backup");
            var sut = new AccountDataStoreFactory(_configMock.Object, _loggerMock.Object);

            var result = sut.Create();

            Assert.IsType<BackupAccountDataStore>(result);
        }
    }
}
