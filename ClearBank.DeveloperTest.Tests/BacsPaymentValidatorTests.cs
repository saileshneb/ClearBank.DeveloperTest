using ClearBank.DeveloperTest.Services.Validators;
using ClearBank.DeveloperTest.Types;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class BacsPaymentValidatorTests
    {
        private readonly BacsPaymentValidator _sut = new BacsPaymentValidator();
        private readonly MakePaymentRequest _request = new MakePaymentRequest { PaymentScheme = PaymentScheme.Bacs };

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
