using ClearBank.DeveloperTest.Types.Domain;
using Xunit;

namespace ClearBank.DeveloperTest.Tests;

public class AccountTests
{
    private static Account MakeAccount(decimal balance = 100m) =>
        new("ACC1", balance, AccountStatus.Live, AllowedPaymentSchemes.Bacs);

    // --- ValidateWithdraw ---

    [Fact]
    public void GivenZeroAmount_WhenValidateWithdrawCalled_ReturnsError()
    {
        var account = MakeAccount();

        var result = account.ValidateWithdraw(0m);

        Assert.False(result.Success);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public void GivenNegativeAmount_WhenValidateWithdrawCalled_ReturnsError()
    {
        var account = MakeAccount();

        var result = account.ValidateWithdraw(-1m);

        Assert.False(result.Success);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public void GivenSufficientFunds_WhenValidateWithdrawCalled_ReturnsSuccess()
    {
        var account = MakeAccount(balance: 100m);

        var result = account.ValidateWithdraw(50m);

        Assert.True(result.Success);
    }

    // --- ValidateDeposit ---

    [Fact]
    public void GivenZeroAmount_WhenValidateDepositCalled_ReturnsError()
    {
        var account = MakeAccount();

        var result = account.ValidateDeposit(0m);

        Assert.False(result.Success);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public void GivenNegativeAmount_WhenValidateDepositCalled_ReturnsError()
    {
        var account = MakeAccount();

        var result = account.ValidateDeposit(-5m);

        Assert.False(result.Success);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public void GivenPositiveAmount_WhenValidateDepositCalled_ReturnsSuccess()
    {
        var account = MakeAccount();

        var result = account.ValidateDeposit(25m);

        Assert.True(result.Success);
    }

    // --- Withdraw / Deposit ---

    [Fact]
    public void Withdraw_DecreasesBalance()
    {
        var account = MakeAccount(balance: 100m);

        account.Withdraw(40m);

        Assert.Equal(60m, account.Balance);
    }

    [Fact]
    public void Deposit_IncreasesBalance()
    {
        var account = MakeAccount(balance: 100m);

        account.Deposit(25m);

        Assert.Equal(125m, account.Balance);
    }
}
