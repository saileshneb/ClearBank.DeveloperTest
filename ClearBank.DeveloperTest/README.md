### Test Description
In the 'PaymentService.cs' file you will find a method for making a payment. At a high level the steps for making a payment are:

 - Lookup the account the payment is being made from
 - Check the account is in a valid state to make the payment
 - Deduct the payment amount from the account's balance and update the account in the database

What we'd like you to do is refactor the code with the following things in mind:
 - Adherence to SOLID principals
 - Testability
 - Readability

We'd also like you to add some unit tests to the ClearBank.DeveloperTest.Tests project to show how you would test the code that you've produced. The only specific 'rules' are:

 - The solution should build.
 - The tests should all pass.
 - You should not change the method signature of the MakePayment method.

You are free to use any frameworks/NuGet packages that you see fit.

You should plan to spend around 1 to 3 hours to complete the exercise.

---

### Implementation

A .NET 8 solution implementing a payment processing service, refactored with SOLID principles, testability, and readability in mind.

#### Architecture

**`PaymentService`** orchestrates a payment by:
1. Resolving the correct data store via `IAccountDataStoreFactory` (driven by configuration)
2. Looking up debtor and creditor accounts
3. Delegating payment scheme validation to `IPaymentRequestValidator`
4. Validating withdraw and deposit amounts on the `Account` domain object
5. Executing the transfer within a unit-of-work transaction

**Key design decisions:**

- **Strategy pattern for validators** — Each payment scheme (`Bacs`, `Chaps`, `FasterPayments`) is a separate `IPaymentSchemaValidator` implementation. `PaymentRequestValidator` resolves the correct one by matching `SupportedScheme`, making it trivial to add new schemes without touching existing code (Open/Closed).
- **Factory for data store selection** — `AccountDataStoreFactory` reads `DataStoreType` from configuration and returns either `AccountDataStore` or `BackupAccountDataStore`, keeping infrastructure selection out of the service.
- **Domain validation on `Account`** — `ValidateWithdraw` and `ValidateDeposit` live on the `Account` entity rather than in the service, keeping balance rules cohesive with the data they operate on.
- **Internal DTOs** — `PaymentRequestDto` and `PaymentResultDto` decouple internal processing from the public API types (`MakePaymentRequest` / `MakePaymentResult`). Extension methods in `PaymentRequestResultConverters` handle mapping at the boundary.
- **Unit of work / transaction** — `IUnitOfWork` and `ITransaction` abstract the transactional boundary, making the service testable without a real database.

*Note: Original implementation of MakePayment looked incomplete with only a withdrawl. I decided to add the deposit too as we had all the required information and seems logically correct*
#### Project Structure

```
ClearBank.DeveloperTest/
├── Data/
│   ├── IAccountDataStore.cs          # Data store abstraction
│   ├── IAccountDataStoreFactory.cs   # Factory abstraction
│   ├── AccountDataStore.cs           # Primary data store
│   ├── BackupAccountDataStore.cs     # Backup data store
│   └── AccountDataStoreFactory.cs   # Config-driven factory
├── Extensions/
│   └── PaymentRequestResultConverters.cs  # MakePaymentRequest <-> DTO mappers
├── Services/
│   ├── PaymentService.cs             # Core payment orchestration
│   ├── IPaymentService.cs
│   ├── IUnitOfWork.cs
│   └── Validators/
│       ├── IPaymentRequestValidator.cs
│       ├── IPaymentSchemaValidator.cs
│       ├── PaymentRequestValidator.cs   # Resolves correct schema validator
│       ├── BacsPaymentValidator.cs
│       ├── ChapsPaymentValidator.cs
│       └── FasterPaymentsValidator.cs
└── Types/
    ├── MakePaymentRequest.cs         # Public API input
    ├── MakePaymentResult.cs          # Public API output
    ├── PaymentScheme.cs
    ├── ResultType.cs
    └── Domain/
        ├── Account.cs                # Domain entity with balance logic
        ├── AccountStatus.cs
        ├── AllowedPaymentSchemes.cs
        ├── PaymentRequestDto.cs      # Internal request representation
        └── PaymentResultDto.cs       # Internal result representation
```

Tests use **xUnit** and **Moq**. All dependencies are injected via interfaces and mocked at the test boundary.

#### Running Tests

```bash
dotnet test
```

With coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html"
```
---
## Future Improvements

1. **Transaction Resilience:**
    - Integrate with Polly for retry policies and circuit breakers.

2. **Scheme Validation handlers:**
    - Implement scheme validation handlers.

3. **Account Service:**
    - Implement Account Service to decouple PaymentService from account data store.

4. **Add Money type:**
    - Add a Money type to represent transaction amount and currency.
    
5. **Improve test layout:**
   - Add test fixture class to consolidate common operation like creating accounts or requests.
