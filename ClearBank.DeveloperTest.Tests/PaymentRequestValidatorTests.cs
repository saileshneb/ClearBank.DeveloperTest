using System;
using ClearBank.DeveloperTest.Services.Validators;
using ClearBank.DeveloperTest.Types;
using Moq;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class PaymentRequestValidatorTests
    {
        private readonly Mock<IPaymentSchemaValidator> _schemaValidatorMock;
        private readonly PaymentRequestValidator _sut;

        public PaymentRequestValidatorTests()
        {
            _schemaValidatorMock = new Mock<IPaymentSchemaValidator>();
            _schemaValidatorMock.Setup(v => v.SupportedScheme).Returns(PaymentScheme.Bacs);
            _schemaValidatorMock.Setup(v => v.IsValid(It.IsAny<Account>(), It.IsAny<MakePaymentRequest>())).Returns(true);

            _sut = new PaymentRequestValidator(new[] { _schemaValidatorMock.Object });
        }

        private static MakePaymentRequest Request(decimal amount = 50m, PaymentScheme scheme = PaymentScheme.Bacs) =>
            new MakePaymentRequest("CRED456", "DEB123", amount, DateTime.UtcNow, scheme);

        private static Account GetAccount(decimal balance = 100m) =>
            new Account("DEB123", balance, AccountStatus.Live, AllowedPaymentSchemes.Bacs);

        // --- Amount validation ---

        [Fact]
        public void GivenZeroAmount_WhenValidateCalled_IsInvalid()
        {
            var result = _sut.Validate(GetAccount(), Request(amount: 0m));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Amount must be greater than zero"));
        }

        [Fact]
        public void GivenNegativeAmount_WhenValidateCalled_IsInvalid()
        {
            var result = _sut.Validate(GetAccount(), Request(amount: -10m));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Amount must be greater than zero"));
        }

        // --- Unsupported payment scheme ---

        [Fact]
        public void GivenUnsupportedPaymentScheme_WhenValidateCalled_IsInvalid()
        {
            var result = _sut.Validate(GetAccount(), Request(scheme: PaymentScheme.Chaps));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Chaps"));
        }

        [Fact]
        public void GivenUnsupportedPaymentScheme_WhenValidateCalled_SchemaValidatorIsNeverCalled()
        {
            _sut.Validate(GetAccount(), Request(scheme: PaymentScheme.Chaps));

            _schemaValidatorMock.Verify(v => v.IsValid(It.IsAny<Account>(), It.IsAny<MakePaymentRequest>()), Times.Never);
        }

        [Fact]
        public void GivenZeroAmountAndUnsupportedScheme_WhenValidateCalled_ReturnsBothErrors()
        {
            var result = _sut.Validate(GetAccount(), Request(amount: 0m, scheme: PaymentScheme.Chaps));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Amount must be greater than zero"));
            Assert.Contains(result.Errors, e => e.Contains("Chaps"));
        }

        // --- Schema validator rejects ---

        [Fact]
        public void GivenSchemaValidatorRejectsAccount_WhenValidateCalled_IsInvalid()
        {
            var account = GetAccount();
            var request = Request();
            _schemaValidatorMock.Setup(v => v.IsValid(account, request)).Returns(false);

            var result = _sut.Validate(account, request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("DEB123"));
        }

        // --- Insufficient balance ---

        [Fact]
        public void GivenInsufficientBalance_WhenValidateCalled_IsInvalid()
        {
            var result = _sut.Validate(GetAccount(balance: 10m), Request(amount: 50m));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Insufficient balance") && e.Contains("DEB123"));
        }

        [Fact]
        public void GivenExactBalance_WhenValidateCalled_IsValid()
        {
            var result = _sut.Validate(GetAccount(balance: 50m), Request(amount: 50m));

            Assert.True(result.IsValid);
        }

        // --- Multiple errors ---

        [Fact]
        public void GivenZeroAmountAndSchemaValidatorRejects_WhenValidateCalled_ReturnsBothErrors()
        {
            var account = GetAccount();
            var request = Request(amount: 0m);
            _schemaValidatorMock.Setup(v => v.IsValid(account, request)).Returns(false);

            var result = _sut.Validate(account, request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Amount must be greater than zero"));
            Assert.Contains(result.Errors, e => e.Contains("DEB123"));
        }

        // --- Happy path ---

        [Fact]
        public void GivenValidRequest_WhenValidateCalled_IsValid()
        {
            var result = _sut.Validate(GetAccount(balance: 100m), Request(amount: 50m));

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
    }
}
