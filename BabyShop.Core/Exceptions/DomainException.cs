namespace BabyShop.Core.Exceptions;

public class DomainException : Exception
{
    public DomainException() : base() { }
    public DomainException(string message) : base(message) { }
}