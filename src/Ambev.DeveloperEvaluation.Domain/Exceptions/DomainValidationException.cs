using Ambev.DeveloperEvaluation.Common.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs due to validation failures within the domain layer.
    /// This exception encapsulates a collection of validation error details.
    /// It inherits from DomainException, centralizing domain-specific errors.
    /// </summary>
    public class DomainValidationException : DomainException // Herda de DomainException!
    {
        public IReadOnlyCollection<ValidationErrorDetail> Errors { get; }

        public DomainValidationException(string message, IEnumerable<ValidationErrorDetail> errors)
            : base(message)
        {
            Errors = errors?.ToList() ?? new List<ValidationErrorDetail>();
        }

        public DomainValidationException(IEnumerable<ValidationErrorDetail> errors)
            : this("One or more domain validation errors occurred.", errors)
        {
        }
        public DomainValidationException(string message)
            : this(message, new List<ValidationErrorDetail> { new ValidationErrorDetail { Detail = message, Error = "" } })
        {
        }
    }
}