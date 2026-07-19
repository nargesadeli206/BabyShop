using BabyShop.Core.Entities.Base;

namespace BabyShop.Core.Entities;

public class TicketReply : BaseEntity
{
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsAdminReply { get; set; }

    public virtual SupportTicket? Ticket { get; set; }
    public virtual User? User { get; set; }
}