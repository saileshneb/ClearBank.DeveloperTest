using System.Collections.Generic;

namespace ClearBank.DeveloperTest.Types.Domain;

public class PaymentResultDto
{
    public bool Success { get; private set; }
    public string ErrorMessage { get; private set; }
    public ResultType ResultType { get; private set; }

    public static PaymentResultDto SuccessResponse()
    {
        return new PaymentResultDto
        {
            Success = true,
            ResultType = ResultType.Success,
        };
    }

    public static PaymentResultDto Error(string errorMessage)
    {
        return new PaymentResultDto
        {
            Success = false,
            ResultType = ResultType.Errored,
            ErrorMessage = errorMessage
        };
    }
}