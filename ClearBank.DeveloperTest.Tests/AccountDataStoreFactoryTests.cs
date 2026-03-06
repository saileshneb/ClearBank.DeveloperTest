using ClearBank.DeveloperTest.Data;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class AccountDataStoreFactoryTests
    {
        [Fact]
        public void Create_ConfigMissing_ReturnsAccountDataStore()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["DataStoreType"]).Returns((string)null);
            var sut = new AccountDataStoreFactory(configMock.Object);

            var result = sut.Create();

            Assert.IsType<AccountDataStore>(result);
        }

        [Fact]
        public void Create_ConfigUnrecognised_ReturnsAccountDataStore()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["DataStoreType"]).Returns("Primary");
            var sut = new AccountDataStoreFactory(configMock.Object);

            var result = sut.Create();

            Assert.IsType<AccountDataStore>(result);
        }

        [Fact]
        public void Create_ConfigBackup_ReturnsBackupAccountDataStore()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["DataStoreType"]).Returns("Backup");
            var sut = new AccountDataStoreFactory(configMock.Object);

            var result = sut.Create();

            Assert.IsType<BackupAccountDataStore>(result);
        }
    }
}
