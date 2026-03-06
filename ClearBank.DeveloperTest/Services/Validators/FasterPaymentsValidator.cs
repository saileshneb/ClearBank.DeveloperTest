using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public class FasterPaymentsValidator : IPaymentSchemaValidator
    {
        public PaymentScheme SupportedScheme => PaymentScheme.FasterPayments;

        public bool IsValid(Account account, MakePaymentRequest request)
        {
            return account != null
                && account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments);
                
        }
    }
}
