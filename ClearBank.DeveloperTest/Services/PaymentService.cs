using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services.Validators;
using ClearBank.DeveloperTest.Types;
using Microsoft.Extensions.Logging;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStoreFactory _dataStoreFactory;
        private readonly IPaymentRequestValidator _paymentRequestValidator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IAccountDataStoreFactory dataStoreFactory,
            IPaymentRequestValidator paymentRequestValidator,
            IUnitOfWork unitOfWork,
            ILogger<PaymentService> logger)
        {
            ArgumentNullException.ThrowIfNull(dataStoreFactory);
            ArgumentNullException.ThrowIfNull(paymentRequestValidator);
            ArgumentNullException.ThrowIfNull(unitOfWork);
            ArgumentNullException.ThrowIfNull(logger);

            _dataStoreFactory = dataStoreFactory;
            _paymentRequestValidator = paymentRequestValidator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var dataStore = _dataStoreFactory.Create();
            var fromAccount = dataStore.GetAccount(request.DebtorAccountNumber);
            var toAccount = dataStore.GetAccount(request.CreditorAccountNumber);

            if (fromAccount is null)
                return MakePaymentResult.Error("Debtor Account not found.");

            if (toAccount is null)
                return MakePaymentResult.Error("Creditor Account not found.");

            var validationResult = _paymentRequestValidator.Validate(fromAccount, request);
            
            if(validationResult.IsValid is false)
            {
                return MakePaymentResult.ValidationFailedResponse("Request validation failed", validationResult.Errors);
            }

            try
            {
                using (var transaction = _unitOfWork.BeginTransaction())
                {
                    fromAccount.Withdraw(request.Amount);
                    toAccount.Deposit(request.Amount);
                    dataStore.UpdateAccount(fromAccount);
                    dataStore.UpdateAccount(toAccount);
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making payment from {DebtorAccountNumber} to {CreditorAccountNumber}",
                    request.DebtorAccountNumber, request.CreditorAccountNumber);

                return MakePaymentResult.Error("Error making payment");
            }

            return MakePaymentResult.SuccessResponse();
        }
    }
}
