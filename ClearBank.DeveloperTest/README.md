### Test Description
In the 'PaymentService.cs' file you will find a method for making a payment. At a high level the steps for making a payment are:

 - Lookup the account the payment is being made from
 - Check the account is in a valid state to make the payment
 - Deduct the payment amount from the account's balance and update the account in the database
 
What we’d like you to do is refactor the code with the following things in mind:  
 - Adherence to SOLID principals
 - Testability  
 - Readability 

We’d also like you to add some unit tests to the ClearBank.DeveloperTest.Tests project to show how you would test the code that you’ve produced. The only specific ‘rules’ are:  

 - The solution should build.
 - The tests should all pass.
 - You should not change the method signature of the MakePayment method.

You are free to use any frameworks/NuGet packages that you see fit.  
 
You should plan to spend around 1 to 3 hours to complete the exercise.

### This solution has the following implementation

A .NET 8 solution implementing a payment processing service with full unit test coverage, refactored with SOLID principles, testability, and readability in mind.

## Original Brief

Refactor `PaymentService` to adhere to SOLID principles, improve testability and readability, and add unit tests. Constraints:

- Solution must build
- All tests must pass
- `MakePayment` method signature must not change

## Solution Structure

```
ClearBank.DeveloperTest/          # Main library
ClearBank.DeveloperTest.Tests/    # Unit tests
```

## Domain Overview

The solution models a payment transfer between two bank accounts — a **debtor** (paying) account and a **creditor** (receiving) account — across three supported payment schemes: **Bacs**, **Chaps**, and **FasterPayments**.

### Payment Flow

```
MakePayment(request)
  │
  ├─ Fetch debtor and creditor accounts from data store
  ├─ Return error if either account not found
  │
  ├─ Validate request via PaymentRequestValidator
  │   ├─ Amount must be greater than zero
  │   ├─ Payment scheme must be supported
  │   ├─ Scheme-specific validation (via IPaymentSchemaValidator)
  │   └─ Debtor account must have sufficient balance
  │
  ├─ Return validation errors if invalid
  │
  └─ Execute within a transaction
      ├─ Withdraw amount from debtor account
      ├─ Deposit amount to creditor account
      ├─ Persist both account updates
      └─ Commit transaction
```

## Key Abstractions

| Interface | Responsibility |
|---|---|
| `IPaymentService` | Entry point for initiating a payment |
| `IPaymentRequestValidator` | Orchestrates all validation rules for a payment request |
| `IPaymentSchemaValidator` | Scheme-specific validation rule (one per payment scheme) |
| `IAccountDataStore` | Reads and writes account data |
| `IAccountDataStoreFactory` | Creates the appropriate data store based on configuration |
| `IUnitOfWork` / `ITransaction` | Wraps account updates in an atomic transaction |

## Payment Schemes

| Scheme | Validator | Rules |
|---|---|---|
| Bacs | `BacsPaymentValidator` | Debtor account must have Bacs enabled |
| Chaps | `ChapsPaymentValidator` | Debtor account must have Chaps enabled and status must be `Live` |
| FasterPayments | `FasterPaymentsValidator` | Debtor account must have FasterPayments enabled |

Amount and balance validation applies to all schemes and is enforced centrally by `PaymentRequestValidator`.

## Data Store Selection

The active data store is selected at runtime via configuration:

| `DataStoreType` config value | Data store used |
|---|---|
| `"Backup"` | `BackupAccountDataStore` |
| Any other value / missing | `AccountDataStore` (default) |

## Result Types

`MakePaymentResult` returns one of three outcomes:

| `ResultType` | `Success` | Meaning |
|---|---|---|
| `Success` | `true` | Payment processed successfully |
| `ValidationFailed` | `false` | Request failed validation; `ValidationErrors` populated |
| `Errored` | `false` | Unexpected error during processing; `ErrorMessage` populated |

## Projects

### `ClearBank.DeveloperTest`

- **Target framework:** .NET 8
- **Dependencies:** `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`

### `ClearBank.DeveloperTest.Tests`

- **Target framework:** .NET 8
- **Test framework:** xUnit
- **Mocking:** Moq

## Running the Tests

```bash
dotnet test
```

## Design Decisions

- **`Account` encapsulation** — properties have private setters; state changes go through `Withdraw(amount)` and `Deposit(amount)` domain methods, preventing external mutation.
- **Immutable request** — `MakePaymentRequest` is a `readonly record struct`, making it safe to pass across boundaries without defensive copying.
- **Strategy pattern for validators** — `IPaymentSchemaValidator` implementations are registered as a collection. Adding a new payment scheme requires only a new validator class with no changes to `PaymentRequestValidator` or `PaymentService`.
- **Separation of validation concerns** — generic rules (amount, balance) live in `PaymentRequestValidator`; scheme-specific rules live in each `IPaymentSchemaValidator`.
- **Structured logging** — `ILogger<PaymentService>` is injected and used with named properties on the exception path, compatible with log aggregation tools (Serilog, Application Insights, etc.).
- **Unit of Work pattern** — account updates are wrapped in `IUnitOfWork` / `ITransaction`, keeping persistence concerns out of the service and enabling transactional rollback.

