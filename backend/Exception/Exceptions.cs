namespace Backend.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
        public BusinessRuleException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
    }

    public class AuthorizationException : Exception
    {
        public string? RequiredRole { get; }
        public string? UserRole { get; }

        public AuthorizationException(string message, string? requiredRole = null, string? userRole = null)
            : base(message)
        {
            RequiredRole = requiredRole;
            UserRole = userRole;
        }
    }
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(string message, List<string> errors) : base(message)
        {
            Errors = errors ?? new List<string>();
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }
    }

    
}