namespace EkubCircle.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

public class EkubRuleViolationException : DomainException
{
    public EkubRuleViolationException(string message) : base(message)
    {
    }
}
