namespace BabyShop.Application.Dtos;

public class SupportTicketDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TicketReplyDto> Replies { get; set; } = new();
}

public class TicketReplyDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsAdminReply { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTicketDto
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
}

public class ReplyToTicketDto
{
    public int TicketId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UpdateTicketStatusDto
{
    public int TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateTicketPriorityDto
{
    public int TicketId { get; set; }
    public string Priority { get; set; } = string.Empty;
}