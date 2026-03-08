using System;
using ClearBank.DeveloperTest.Services.Validators;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class BacsPaymentValidatorTests
    {  
        private readonly BacsPaymentValidator _sut = new();

        private readonly MakePaymentRequest _request = new(
            "1234",
            "3456",
            10m,
            DateTime.UtcNow,
            PaymentScheme.Bacs);
       

        [Fact]
        public void GivenBacsValidator_SupportedScheme_IsBacs()
        {
            Assert.Equal(PaymentScheme.Bacs, _sut.SupportedScheme);
        }

        [Fact]
        public void GivenNullAccount_WhenIsValidCalled_ReturnsFalse()
        {
            Assert.False(_sut.IsValid(null, _request));
        }

        [Fact]
        public void GivenUnsupportedScheme_WhenIsValidCalled_ReturnsFalse()
        {
            var account = new Account("", 0m, AccountStatus.Live, AllowedPaymentSchemes.FasterPayments);
            Assert.False(_sut.IsValid(account, _request));
        }

        [Fact]
        public void GivenBacsSchema_WhenIsValidCalled_ReturnsTrue()
        {
            var account = new Account("", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            Assert.True(_sut.IsValid(account, _request));
        }

        [Fact]
        public void GivenMultipleSchemesIncludesBacs_WhenIsValidCalled_ReturnsTrue()
        {
            var account = new Account("", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments);
            Assert.True(_sut.IsValid(account, _request));
        }
    }
}
