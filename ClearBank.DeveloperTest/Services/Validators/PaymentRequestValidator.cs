using ClearBank.DeveloperTest.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using ClearBank.DeveloperTest.Types.Domain;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public class PaymentRequestValidator : IPaymentRequestValidator
    {
        private readonly IEnumerable<IPaymentSchemaValidator> _schemaValidators;
        //private readonly IDateTimeProvider _dateTimeProvider;

        public PaymentRequestValidator(IEnumerable<IPaymentSchemaValidator> schemaValidators)
        {
            ArgumentNullException.ThrowIfNull(schemaValidators);

            _schemaValidators = schemaValidators;
            //_dateTimeProvider = dateTimeProvider;
        }


        public ValidationResult Validate(Account fromAccount, MakePaymentRequest request)
        {
            var errors = new List<string>();

            //do we need to verify paymentDate i.e. can the payment date be in the past?
            //if required then we need IDateTimeProvider to be injected to get the current date and time

            var schemaValidator = _schemaValidators.FirstOrDefault(v => v.SupportedScheme == request.PaymentScheme);

            if (schemaValidator is null)
            {
                errors.Add($"Unsupported payment scheme: {request.PaymentScheme}");
                return new ValidationResult { Errors = errors, IsValid = false };
            }

            if (!schemaValidator.IsValid(fromAccount, request))
            {
                errors.Add($"Payment scheme {request.PaymentScheme} validation failed for Debtor Account {request.DebtorAccountNumber}.");
            }

            return new ValidationResult { Errors = errors, IsValid = !errors.Any() };
        }
    }
}
