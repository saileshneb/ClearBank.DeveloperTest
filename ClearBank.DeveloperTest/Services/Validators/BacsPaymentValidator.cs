using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public class BacsPaymentValidator : IPaymentSchemaValidator
    {
        public PaymentScheme SupportedScheme => PaymentScheme.Bacs;

        public bool IsValid(Account account, MakePaymentRequest request)
        {
            return account != null && account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs);
        }
    }
}
