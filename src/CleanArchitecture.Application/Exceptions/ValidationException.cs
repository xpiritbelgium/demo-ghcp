using FluentValidation.Results;

namespace CleanArchitecture.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(ValidationResult validationResult) : base("Validation failed")
        {
            ValdationErrors = new List<string>();

            foreach (var validationError in validationResult.Errors)
            {
                ValdationErrors.Add(validationError.ErrorMessage);
            }
        }

        public List<string> ValdationErrors { get; set; }
    }
}
