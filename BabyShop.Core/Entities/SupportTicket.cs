using BabyShop.Core.Entities.Base;

namespace BabyShop.Core.Entities;

public class SupportTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public string Category { get; set; } = "General";

    public virtual User? User { get; set; }
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
    public virtual ICollection<TicketReply> Replies { get; set; } = new List<TicketReply>();
}