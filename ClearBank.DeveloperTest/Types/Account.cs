namespace ClearBank.DeveloperTest.Types
{
    public class Account
    {
        public string AccountNumber { get; private set; }
        public decimal Balance { get; private set; } //This could be a type like Money. For simplicity, we are using decimal here.
        public AccountStatus Status { get; private set; }
        public AllowedPaymentSchemes AllowedPaymentSchemes { get; private set; }

        public Account(string accountNumber, decimal balance, AccountStatus accountStatus, AllowedPaymentSchemes allowedPaymentSchemes )
        {
            AccountNumber = accountNumber;
            Balance = balance;
            Status = accountStatus;
            AllowedPaymentSchemes = allowedPaymentSchemes;
        }

        public void Withdraw(decimal amount)
        {
            Balance -= amount;
        }

        public void Deposit(decimal amount)
        {
            Balance += amount;
        }
    }
}
