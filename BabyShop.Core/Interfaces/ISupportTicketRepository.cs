using BabyShop.Core.Entities;

namespace BabyShop.Core.Interfaces;

public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAsync(int id);
    Task<IReadOnlyList<SupportTicket>> GetByUserIdAsync(int userId);
    Task<IReadOnlyList<SupportTicket>> GetAllAsync();
    Task<SupportTicket> AddAsync(SupportTicket ticket);
    Task UpdateAsync(SupportTicket ticket);
    Task<TicketReply> AddReplyAsync(TicketReply reply);
    Task<IReadOnlyList<TicketReply>> GetRepliesAsync(int ticketId);
    Task<int> CountByStatusAsync(string status);
    Task<int> CountAllAsync();
    Task<string> GenerateTicketNumberAsync();
}