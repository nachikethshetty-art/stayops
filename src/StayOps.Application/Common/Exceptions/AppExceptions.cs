namespace StayOps.Application.Common.Exceptions;

public class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with key '{key}' was not found.");

public class ForbiddenAccessException(string message = "You do not have access to this resource.")
    : Exception(message);

public class ConflictException(string message) : Exception(message);

public class InvalidCredentialsException(string message = "Invalid username/email or password.") : Exception(message);

/// <summary>Thrown for business-rule violations that should surface as HTTP 422/409 ProblemDetails, e.g. overbooking, invalid state transitions.</summary>
public class BusinessRuleException(string message) : Exception(message);

public class ValidationAppException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException() : base("One or more validation failures occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationAppException(IEnumerable<FluentValidation.Results.ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
