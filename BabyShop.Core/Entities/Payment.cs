using BabyShop.Core.Entities.Base;
using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Payment : BaseEntity
{
    public int OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? Authority { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public string? RefundReason { get; private set; }

    public virtual Order? Order { get; set; }

    public bool IsPaid => Status == "Paid";
    public bool IsRefunded => Status == "Refunded";
    public bool IsPending => Status == "Pending";
    public bool IsFailed => Status == "Failed";

    private Payment() { }

    public Payment(int orderId, decimal amount, string paymentMethod)
    {
        if (orderId <= 0)
            throw new BusinessRuleException("OrderId is required");
        if (amount <= 0)
            throw new BusinessRuleException("Amount must be greater than zero");
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new BusinessRuleException("Payment method is required");

        OrderId = orderId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Status = "Pending";
        CreatedAt = DateTime.UtcNow;
    }

    public void SetAuthority(string authority)
    {
        if (string.IsNullOrWhiteSpace(authority))
            throw new BusinessRuleException("Authority is required");

        Authority = authority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Verify(string referenceNumber)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
            throw new BusinessRuleException("Reference number is required");
        if (Status != "Pending")
            throw new BusinessRuleException("Only pending payments can be verified");

        Status = "Paid";
        ReferenceNumber = referenceNumber;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Refund(string reason)
    {
        if (Status != "Paid")
            throw new BusinessRuleException("Only paid payments can be refunded");
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("Refund reason is required");

        Status = "Refunded";
        RefundedAt = DateTime.UtcNow;
        RefundReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (Status != "Pending")
            throw new BusinessRuleException("Only pending payments can fail");

        Status = "Failed";
        UpdatedAt = DateTime.UtcNow;
    }
}