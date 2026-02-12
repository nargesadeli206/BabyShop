namespace BabyShop.Core.Exceptions;

/// <summary>
/// استثنای قانون کسب و کار - وقتی یک قانون نقض می‌شود
/// </summary>
public sealed class BusinessRuleException : Exception
{
    public string Rule { get; }

    public BusinessRuleException(string rule, string message) : base(message)
    {
        Rule = rule;
    }

    public BusinessRuleException(string rule, string message, Exception innerException)
        : base(message, innerException)
    {
        Rule = rule;
    }
}