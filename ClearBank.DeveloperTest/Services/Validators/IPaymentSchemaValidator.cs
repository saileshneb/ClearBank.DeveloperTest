using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public interface IPaymentSchemaValidator
    {
        PaymentScheme SupportedScheme { get; }
        bool IsValid(Account account, MakePaymentRequest request);
    }
}
