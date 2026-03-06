using System;

namespace ClearBank.DeveloperTest.Types
{
    public readonly record struct MakePaymentRequest(
        string CreditorAccountNumber,
        string DebtorAccountNumber,
        decimal Amount,
        DateTime PaymentDate,
        PaymentScheme PaymentScheme
    );
}
