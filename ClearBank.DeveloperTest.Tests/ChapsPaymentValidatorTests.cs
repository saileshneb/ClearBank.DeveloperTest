using System;
using ClearBank.DeveloperTest.Services.Validators;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class ChapsPaymentValidatorTests
    {
        private readonly ChapsPaymentValidator _sut = new ChapsPaymentValidator();
        private readonly MakePaymentRequest _request = new(
            "1234",
            "3456",
            10m,
            DateTime.UtcNow,
            PaymentScheme.Chaps);

        [Fact]
        public void GivenChapsValidator_SupportedScheme_IsChaps()
        {
            Assert.Equal(PaymentScheme.Chaps, _sut.SupportedScheme);
        }

        [Fact]
        public void GivenNullAccount_WhenIsValidIsCalled_ReturnsFalse()
        {
            Assert.False(_sut.IsValid(null, _request));
        }

        [Fact]
        public void GivenSchemeNotAllowed_WhenIsValidIsCalled_ReturnsFalse()
        {
            var account = new Account("", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            Assert.False(_sut.IsValid(account, _request));
        }

        [Theory]
        [InlineData(AccountStatus.Disabled)]
        [InlineData(AccountStatus.InboundPaymentsOnly)]
        public void GivenAccountStatusNotAllowed_WhenIsValidIsCalled_ReturnsFalse(AccountStatus accountStatus)
        {
            var account = new Account("", 0m, accountStatus, AllowedPaymentSchemes.Chaps);
            Assert.False(_sut.IsValid(account, _request));
        }

        [Fact]
        public void GivenAccountStatusLive_WhenIsValidCalled_ReturnsTrue()
        {
            var account = new Account("", 0m, AccountStatus.Live, AllowedPaymentSchemes.Chaps);
            Assert.True(_sut.IsValid(account, _request));
        }
    }
}
