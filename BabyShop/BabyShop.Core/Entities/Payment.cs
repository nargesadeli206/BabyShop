using BabyShop.Core.Exceptions;

namespace BabyShop.Core.Entities;

public class Payment : Basket
{
    public int OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Authority { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";
    public string? TransactionId { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public Order? Order { get; set; }

    private Payment() { }

    public Payment(int orderId, decimal amount)
    {
        OrderId = orderId;
        Amount = amount;
        Authority = Guid.NewGuid().ToString("N").ToUpper();
        Status = "Pending";
    }

    public void MarkAsPaid(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new BusinessRuleException("Transaction ID cannot be empty.");
        if (Status != "Pending")
            throw new BusinessRuleException($"Payment is already {Status}.");

        Status = "Paid";
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = "Failed";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRefunded()
    {
        if (Status != "Paid")
            throw new BusinessRuleException("Only paid payments can be refunded.");

        Status = "Refunded";
        UpdatedAt = DateTime.UtcNow;
    }
}