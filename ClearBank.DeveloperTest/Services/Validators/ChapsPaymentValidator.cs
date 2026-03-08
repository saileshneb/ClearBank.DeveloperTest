using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public class ChapsPaymentValidator : IPaymentSchemaValidator
    {
        public PaymentScheme SupportedScheme => PaymentScheme.Chaps;

        public bool IsValid(Account account, MakePaymentRequest request)
        {
            return account != null
                && account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps)
                && account.Status == AccountStatus.Live;
        }
    }
}
