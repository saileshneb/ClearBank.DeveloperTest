using System;
using ClearBank.DeveloperTest.Services.Validators;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;
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


        // --- Unsupported payment scheme ---

        [Fact]
        public void GivenUnsupportedPaymentScheme_WhenValidateCalled_IsInvalid()
        {
            var result = _sut.Validate(GetAccount(), Request(scheme: PaymentScheme.Chaps));

            Assert.False(result.Success);
            Assert.Contains(result.ValidationErrors, e => e.Contains("Chaps"));
        }

        [Fact]
        public void GivenUnsupportedPaymentScheme_WhenValidateCalled_SchemaValidatorIsNeverCalled()
        {
            _sut.Validate(GetAccount(), Request(scheme: PaymentScheme.Chaps));

            _schemaValidatorMock.Verify(v => v.IsValid(It.IsAny<Account>(), It.IsAny<MakePaymentRequest>()), Times.Never);
        }


        // --- Multiple errors ---

        [Fact]
        public void GivenSchemaValidatorRejects_WhenValidateCalled_ReturnsErrors()
        {
            var account = GetAccount();
            var request = Request(amount: 10m);
            _schemaValidatorMock.Setup(v => v.IsValid(account, request)).Returns(false);

            var result = _sut.Validate(account, request);

            Assert.False(result.Success);
            Assert.Contains(result.ValidationErrors, e => e.Contains("DEB123"));
        }

        // --- Happy path ---

        [Fact]
        public void GivenValidRequest_WhenValidateCalled_IsValid()
        {
            var result = _sut.Validate(GetAccount(balance: 100m), Request(amount: 50m));

            Assert.True(result.Success);
            Assert.Empty(result.ValidationErrors);
        }
    }
}
