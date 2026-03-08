using ClearBank.DeveloperTest.Types;
using ClearBank.DeveloperTest.Types.Domain;
using Xunit;

namespace ClearBank.DeveloperTest.Tests;

public class PaymentResultDtoTests
{
    [Fact]
    public void SuccessResponse_SetsSuccessTrueAndResultTypeSuccess()
    {
        var result = PaymentResultDto.SuccessResponse();

        Assert.True(result.Success);
        Assert.Equal(ResultType.Success, result.ResultType);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Error_SetsSuccessFalseResultTypeErroredAndStoresMessage()
    {
        var result = PaymentResultDto.Error("bad input");

        Assert.False(result.Success);
        Assert.Equal(ResultType.Errored, result.ResultType);
        Assert.Equal("bad input", result.ErrorMessage);
    }
}
