using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Extensions;
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
            //probably offload this to an account service
            var dataStore = _dataStoreFactory.Create();
            var fromAccount = dataStore.GetAccount(request.DebtorAccountNumber);
            var toAccount = dataStore.GetAccount(request.CreditorAccountNumber);

            if (fromAccount is null)
                return MakePaymentResult.Error("Debtor Account not found.");

            if (toAccount is null)
                return MakePaymentResult.Error("Creditor Account not found.");

            var validationResult = _paymentRequestValidator.Validate(fromAccount, request);
            
            if(validationResult.Success is false)
            {
                return validationResult;
            }
            
            var validateWithdrawResult = fromAccount.ValidateWithdraw(request.Amount);
            if(!validateWithdrawResult.Success)
                return validateWithdrawResult.ToResult();
            
            var validateDepositResult = toAccount.ValidateDeposit(request.Amount);
            if(!validateDepositResult.Success)
                return validateDepositResult.ToResult();

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
