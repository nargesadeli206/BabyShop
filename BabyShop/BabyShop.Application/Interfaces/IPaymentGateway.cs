public interface IPaymentGateway
{
    Task<string> RequestPayment(decimal amount, string callbackUrl);
    Task<bool> VerifyPayment(string authority, decimal amount);
}
