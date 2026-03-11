using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public interface IPaymentRequestValidator
    {
        ValidationResult Validate(Account fromAccount, MakePaymentRequest request);
    }
}
