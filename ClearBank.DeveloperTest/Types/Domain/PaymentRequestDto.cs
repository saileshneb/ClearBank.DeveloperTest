using System;

namespace ClearBank.DeveloperTest.Types.Domain;

public readonly record struct PaymentRequestDto(
    string CreditorAccountNumber,
    string DebtorAccountNumber,
    decimal Amount,
    DateTime PaymentDate,
    PaymentScheme PaymentScheme
);