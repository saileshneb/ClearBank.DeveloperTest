using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;

namespace ClearBank.DeveloperTest.Extensions;

public static class PaymentRequestResultConverters
{
    public static PaymentRequestDto ToDto(this MakePaymentRequest request)
    {
        return new PaymentRequestDto(request.CreditorAccountNumber,
            request.DebtorAccountNumber,
            request.Amount,
            request.PaymentDate,
            request.PaymentScheme);
    }

    public static MakePaymentResult ToResult(this PaymentResultDto resultDto)
    {
        return new MakePaymentResult(resultDto.Success,
            resultDto.ResultType,
            resultDto.ErrorMessage,
            []);
    }
}

