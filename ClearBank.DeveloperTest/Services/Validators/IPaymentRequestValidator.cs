using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public interface IPaymentRequestValidator
    {
        MakePaymentResult Validate(Account fromAccount, MakePaymentRequest request);
    }
}
