using System.Collections.Generic;

namespace ClearBank.DeveloperTest.Types
{
    public class MakePaymentResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }
        public ResultType ResultType { get;  }
        public List<string> ValidationErrors { get;  }

        public MakePaymentResult(bool success, 
            ResultType resultType,
            string errorMessage = null,
            List<string> validationErrors = null)
        {
            Success = success;
            ErrorMessage = errorMessage;
            ResultType = resultType;
            ValidationErrors = validationErrors ?? [];
        }

        public static MakePaymentResult SuccessResponse()
        {
            return new MakePaymentResult(success: true, resultType: ResultType.Success);
        }
        
        public static MakePaymentResult ValidationFailedResponse(string errorMessage, List<string> validationErrors)
        {
            return new MakePaymentResult(success: false, 
                resultType: ResultType.ValidationFailed,
                errorMessage: errorMessage,
                validationErrors: validationErrors);
        }
        
        public static MakePaymentResult Error(string errorMessage)
        {
            return new MakePaymentResult(success: false, 
                resultType: ResultType.Errored,
                errorMessage: errorMessage);
        }
    }
}
