using System.Text.Json.Serialization;

namespace ClearBank.DeveloperTest.Types
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum PaymentScheme
    {
        FasterPayments,
        Bacs,
        Chaps
    }
}
