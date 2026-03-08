using System;
using ClearBank.DeveloperTest.Extensions;
using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;
using Xunit;

namespace ClearBank.DeveloperTest.Tests;

public class PaymentRequestResultConvertersTests
{
    [Fact]
    public void ToDto_MapsAllFieldsCorrectly()
    {
        var date = DateTime.UtcNow;
        var request = new MakePaymentRequest("CRED1", "DEB1", 50m, date, PaymentScheme.Bacs);

        var dto = request.ToDto();

        Assert.Equal("CRED1", dto.CreditorAccountNumber);
        Assert.Equal("DEB1", dto.DebtorAccountNumber);
        Assert.Equal(50m, dto.Amount);
        Assert.Equal(date, dto.PaymentDate);
        Assert.Equal(PaymentScheme.Bacs, dto.PaymentScheme);
    }

    [Fact]
    public void ToResult_GivenSuccessDto_MapsToSuccessResult()
    {
        var dto = PaymentResultDto.SuccessResponse();

        var result = dto.ToResult();

        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.ResultType);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ToResult_GivenErrorDto_MapsErrorMessageAndResultType()
    {
        var dto = PaymentResultDto.Error("something went wrong");

        var result = dto.ToResult();

        Assert.False(result.Success);
        Assert.Equal(ResultType.Errored, result.ResultType);
        Assert.Equal("something went wrong", result.ErrorMessage);
    }
}
