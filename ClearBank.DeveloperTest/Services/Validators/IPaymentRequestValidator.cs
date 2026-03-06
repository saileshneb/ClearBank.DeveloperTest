using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public interface IPaymentRequestValidator
    {
        ValidationResult Validate(Account fromAccount, MakePaymentRequest request);
    }
}
