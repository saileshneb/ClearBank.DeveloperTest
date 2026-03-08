using System;
using ClearBank.DeveloperTest.Services.Validators;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class FasterPaymentsValidatorTests
    {
        private readonly FasterPaymentsValidator _sut = new FasterPaymentsValidator();

        private MakePaymentRequest Request(decimal amount = 50m) =>
        new MakePaymentRequest(
            "1234",
            "3456",
            amount,
            DateTime.UtcNow,
            PaymentScheme.FasterPayments);

        [Fact]
        public void GivenFasterPaymentValidator_SupportedScheme_IsFasterPayments()
        {
            Assert.Equal(PaymentScheme.FasterPayments, _sut.SupportedScheme);
        }

        [Fact]
        public void GivenNullAccount_WhenIsValidCalled_ReturnsFalse()
        {
            Assert.False(_sut.IsValid(null, Request()));
        }

        [Theory]
        [InlineData(AllowedPaymentSchemes.Bacs)]
        [InlineData(AllowedPaymentSchemes.Chaps)]
        public void GivenSchemeNotAllowed_WhenIsValidCalled_ReturnsFalse(AllowedPaymentSchemes schemes)
        {
            var account = new Account("", 100m, AccountStatus.Live, schemes);
            Assert.False(_sut.IsValid(account, Request()));
        }

        [Fact]
        public void GivenExactBalance_WhenIsValidCalled_ReturnsTrue()
        {
            var account = new Account("", 50m, AccountStatus.Live, AllowedPaymentSchemes.FasterPayments);
            Assert.True(_sut.IsValid(account, Request(50m)));
        }

        [Fact]
        public void GivenSufficientBalance_WhenIsValidCalled_ReturnsTrue()
        {
            var account = new Account("", 100m, AccountStatus.Live, AllowedPaymentSchemes.FasterPayments);
            Assert.True(_sut.IsValid(account, Request(50m)));
        }
    }
}
