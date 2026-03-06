using System;

namespace ClearBank.DeveloperTest
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

    }
}
