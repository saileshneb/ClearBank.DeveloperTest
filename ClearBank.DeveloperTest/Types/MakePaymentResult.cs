using System.Collections.Generic;

namespace ClearBank.DeveloperTest.Types
{
    public sealed class MakePaymentResult
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }
        public ResultType ResultType { get; private set; }
        public List<string> ValidationErrors { get; private set; } = new();

        public static MakePaymentResult SuccessResponse()
        {
            return new MakePaymentResult
            {
                Success = true,
                ResultType = ResultType.Success,
            };
        }

        public static MakePaymentResult ValidationFailedResponse(string errorMessage, List<string> validationErrors)
        {
            return new MakePaymentResult
            {
                Success = false,
                ResultType = ResultType.ValidationFailed,
                ErrorMessage = errorMessage,
                ValidationErrors = validationErrors
            };
        }

        public static MakePaymentResult Error(string errorMessage)
        {
            return new MakePaymentResult
            {
                Success = false,
                ResultType = ResultType.Errored,
                ErrorMessage = errorMessage
            };
        }
    }

    public enum ResultType
    {
        Success,
        ValidationFailed,
        Errored
    }

}
