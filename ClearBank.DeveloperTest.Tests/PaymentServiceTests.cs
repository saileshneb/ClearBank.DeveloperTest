using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Services.Validators;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IAccountDataStoreFactory> _dataStoreFactoryMock;
        private readonly Mock<IAccountDataStore> _accountDataStoreMock;
        private readonly Mock<IPaymentRequestValidator> _paymentRequestValidatorMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITransaction> _transactionMock;
        private readonly Mock<ILogger<PaymentService>> _loggerMock;
        private readonly PaymentService _sut;

        private static readonly MakePaymentRequest DefaultRequest = new MakePaymentRequest(
            CreditorAccountNumber: "CRED456",
            DebtorAccountNumber: "DEB123",
            Amount: 30m,
            PaymentDate: DateTime.UtcNow,
            PaymentScheme: PaymentScheme.Bacs
        );

        public PaymentServiceTests()
        {
            _dataStoreFactoryMock = new Mock<IAccountDataStoreFactory>();
            _accountDataStoreMock = new Mock<IAccountDataStore>();
            _paymentRequestValidatorMock = new Mock<IPaymentRequestValidator>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _transactionMock = new Mock<ITransaction>();
            _loggerMock = new Mock<ILogger<PaymentService>>();

            _dataStoreFactoryMock.Setup(f => f.Create()).Returns(_accountDataStoreMock.Object);
            _unitOfWorkMock.Setup(u => u.BeginTransaction()).Returns(_transactionMock.Object);

            _sut = new PaymentService(_dataStoreFactoryMock.Object, _paymentRequestValidatorMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
        }

        // --- Debtor account not found ---

        [Fact]
        public void GivenDebtorAccountDoesNotExist_WhenMakePaymentCalled_ReturnsError()
        {
            _accountDataStoreMock.Setup(s => s.GetAccount("DEB123")).Returns((Account)null);
            _accountDataStoreMock.Setup(s => s.GetAccount("CRED456")).Returns(new Account("CRED456", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs));

            var result = _sut.MakePayment(DefaultRequest);

            Assert.False(result.Success);
            Assert.Equal(ResultType.Errored, result.ResultType);
            Assert.Contains("Debtor", result.ErrorMessage);
            _accountDataStoreMock.Verify(s => s.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }

        // --- Creditor account not found ---

        [Fact]
        public void GivenCreditorAccountDoesNotExist_WhenMakePaymentCalled_ReturnsError()
        {
            _accountDataStoreMock.Setup(s => s.GetAccount("DEB123")).Returns(new Account("DEB123", 100m, AccountStatus.Live, AllowedPaymentSchemes.Bacs));
            _accountDataStoreMock.Setup(s => s.GetAccount("CRED456")).Returns((Account)null);

            var result = _sut.MakePayment(DefaultRequest);

            Assert.False(result.Success);
            Assert.Equal(ResultType.Errored, result.ResultType);
            Assert.Contains("Creditor", result.ErrorMessage);
            _accountDataStoreMock.Verify(s => s.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }

        // --- Validation fails ---

        [Fact]
        public void GivenValidationFails_WhenMakePaymentCalled_ReturnsValidationFailure()
        {
            var fromAccount = new Account("DEB123", 100m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            var toAccount = new Account("CRED456", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            _accountDataStoreMock.Setup(s => s.GetAccount("DEB123")).Returns(fromAccount);
            _accountDataStoreMock.Setup(s => s.GetAccount("CRED456")).Returns(toAccount);
            _paymentRequestValidatorMock
                .Setup(v => v.Validate(fromAccount, DefaultRequest))
                .Returns(new ValidationResult { IsValid = false, Errors = ["Scheme not supported."] });

            var result = _sut.MakePayment(DefaultRequest);

            Assert.False(result.Success);
            Assert.Equal(ResultType.ValidationFailed, result.ResultType);
            Assert.NotEmpty(result.ValidationErrors);
            _accountDataStoreMock.Verify(s => s.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public void GivenValidationFails_WhenMakePaymentCalled_BalancesAreUnchanged()
        {
            var fromAccount = new Account("DEB123", 100m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            var toAccount = new Account("CRED456", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            _accountDataStoreMock.Setup(s => s.GetAccount("DEB123")).Returns(fromAccount);
            _accountDataStoreMock.Setup(s => s.GetAccount("CRED456")).Returns(toAccount);
            _paymentRequestValidatorMock
                .Setup(v => v.Validate(fromAccount, DefaultRequest))
                .Returns(new ValidationResult { IsValid = false, Errors = ["Scheme not supported."] });

            _sut.MakePayment(DefaultRequest);

            Assert.Equal(100m, fromAccount.Balance);
            Assert.Equal(0m, toAccount.Balance);
        }

        // --- Validation passes (happy path) ---

        [Fact]
        public void GivenValidationPasses_WhenMakePaymentCalled_ReturnsSuccess()
        {
            var fromAccount = new Account("DEB123", 100m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            var toAccount = new Account("CRED456", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            _accountDataStoreMock.Setup(s => s.GetAccount("DEB123")).Returns(fromAccount);
            _accountDataStoreMock.Setup(s => s.GetAccount("CRED456")).Returns(toAccount);
            _paymentRequestValidatorMock
                .Setup(v => v.Validate(fromAccount, DefaultRequest))
                .Returns(new ValidationResult { IsValid = true });

            var result = _sut.MakePayment(DefaultRequest);

            Assert.True(result.Success);
            Assert.Equal(ResultType.Success, result.ResultType);
            _accountDataStoreMock.Verify(s => s.UpdateAccount(fromAccount), Times.Once);
            _accountDataStoreMock.Verify(s => s.UpdateAccount(toAccount), Times.Once);
            _transactionMock.Verify(t => t.Commit(), Times.Once);
        }

        [Fact]
        public void GivenValidationPasses_WhenMakePaymentCalled_WithdrawsFromDebtorAndDepositsToCreditor()
        {
            var fromAccount = new Account("DEB123", 100m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            var toAccount = new Account("CRED456", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            _accountDataStoreMock.Setup(s => s.GetAccount("DEB123")).Returns(fromAccount);
            _accountDataStoreMock.Setup(s => s.GetAccount("CRED456")).Returns(toAccount);
            _paymentRequestValidatorMock
                .Setup(v => v.Validate(fromAccount, DefaultRequest))
                .Returns(new ValidationResult { IsValid = true });

            _sut.MakePayment(DefaultRequest);

            Assert.Equal(70m, fromAccount.Balance);
            Assert.Equal(30m, toAccount.Balance);
        }

        // --- Transaction failure ---

        [Fact]
        public void GivenTransactionThrows_WhenMakePaymentCalled_ReturnsError()
        {
            var fromAccount = new Account("DEB123", 100m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            var toAccount = new Account("CRED456", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            _accountDataStoreMock.Setup(s => s.GetAccount("DEB123")).Returns(fromAccount);
            _accountDataStoreMock.Setup(s => s.GetAccount("CRED456")).Returns(toAccount);
            _paymentRequestValidatorMock
                .Setup(v => v.Validate(fromAccount, DefaultRequest))
                .Returns(new ValidationResult { IsValid = true });

            _accountDataStoreMock
                .SetupSequence(s => s.UpdateAccount(It.IsAny<Account>()))
                .Pass()
               .Throws<Exception>(); //Throws on the 2nd call

            var result = _sut.MakePayment(DefaultRequest);

            Assert.False(result.Success);
            Assert.Equal(ResultType.Errored, result.ResultType);
            Assert.Equal("Error making payment", result.ErrorMessage);
        }

        // --- Account withdrawl / deposit validation ---

        [Fact]
        public void GivenWithdrawlValidationFails_WhenMakePaymentCalled_ReturnsError()
        {
            var zeroAmountRequest = new MakePaymentRequest(
            CreditorAccountNumber: "CRED456",
            DebtorAccountNumber: "DEB123",
            Amount: 0,
            PaymentDate: DateTime.UtcNow,
            PaymentScheme: PaymentScheme.Bacs
        );
            var fromAccount = new Account("DEB123", 10, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            var toAccount = new Account("CRED456", 0m, AccountStatus.Live, AllowedPaymentSchemes.Bacs);
            _accountDataStoreMock.Setup(s => s.GetAccount("DEB123")).Returns(fromAccount);
            _accountDataStoreMock.Setup(s => s.GetAccount("CRED456")).Returns(toAccount);
            _paymentRequestValidatorMock
                .Setup(v => v.Validate(fromAccount, zeroAmountRequest))
                .Returns(new ValidationResult { IsValid = true });

            var result = _sut.MakePayment(zeroAmountRequest);

            Assert.False(result.Success);
            Assert.Equal(ResultType.Errored, result.ResultType);
            Assert.Equal("Amount must be greater than zero", result.ErrorMessage);
        }


        // --- Infrastructure interactions ---

        [Fact]
        public void GivenAnyRequest_WhenMakePaymentCalled_DataStoreFactoryCalledOnce()
        {
            _accountDataStoreMock.Setup(s => s.GetAccount(It.IsAny<string>())).Returns((Account)null);

            _sut.MakePayment(DefaultRequest);

            _dataStoreFactoryMock.Verify(f => f.Create(), Times.Once);
        }

        [Fact]
        public void GivenRequest_WhenMakePaymentCalled_GetsAccountsUsingAccountNumbers()
        {
            _accountDataStoreMock.Setup(s => s.GetAccount(It.IsAny<string>())).Returns((Account)null);

            _sut.MakePayment(DefaultRequest);

            _accountDataStoreMock.Verify(s => s.GetAccount("DEB123"), Times.Once);
            _accountDataStoreMock.Verify(s => s.GetAccount("CRED456"), Times.Once);
        }
    }
}
