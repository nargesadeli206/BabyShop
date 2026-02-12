public class FakePaymentGateway : IPaymentGateway
{
    public Task<string> RequestPayment(decimal amount, string callbackUrl)
    {
        // شبیه‌سازی توکن درگاه
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public Task<bool> VerifyPayment(string authority, decimal amount)
    {
        // شبیه‌سازی پرداخت موفق
        return Task.FromResult(true);
    }
}

