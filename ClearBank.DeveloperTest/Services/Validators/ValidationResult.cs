using System.Collections.Generic;

namespace ClearBank.DeveloperTest.Services.Validators
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}