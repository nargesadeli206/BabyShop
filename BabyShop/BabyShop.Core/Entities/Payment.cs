using BabyShop.Core.Entities;
using BabyShop.Core.Enums;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

/// <summary>
/// موجودیت پرداخت
/// </summary>
public sealed class Payment : BaseEntity
{
    public int OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Authority { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? TransactionId { get; private set; }
    public DateTime? PaidAt { get; private set; }

    // ارتباطات
    public Order Order { get; private set; }

    private Payment() { } // برای EF Core

    public Payment(int orderId, decimal amount)
    {
        OrderId = orderId;
        Amount = amount;
        Authority = GenerateAuthority();
        Status = PaymentStatus.Pending;
    }

    /// <summary>
    /// تایید پرداخت
    /// </summary>
    public void MarkAsPaid(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new DomainException("Transaction ID cannot be empty.");

        Status = PaymentStatus.Paid;
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
        UpdateAuditFields();
    }

    /// <summary>
    /// پرداخت ناموفق
    /// </summary>
    public void MarkAsFailed()
    {
        Status = PaymentStatus.Failed;
        UpdateAuditFields();
    }

    /// <summary>
    /// استرداد وجه
    /// </summary>
    public void MarkAsRefunded()
    {
        if (Status != PaymentStatus.Paid)
            throw new DomainException("Only paid payments can be refunded.");

        Status = PaymentStatus.Refunded;
        UpdateAuditFields();
    }

    private static string GenerateAuthority()
    {
        return Guid.NewGuid().ToString("N");
    }
}