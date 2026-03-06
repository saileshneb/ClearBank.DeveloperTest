using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public interface IPaymentSchemaValidator
    {
        PaymentScheme SupportedScheme { get; }
        bool IsValid(Account account, MakePaymentRequest request);
    }
}
