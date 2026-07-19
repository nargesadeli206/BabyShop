using BabyShop.Application.Dtos;

namespace BabyShop.Application.Interfaces.Services;

public interface ISupportTicketService
{
    Task<List<SupportTicketDto>> GetMyTicketsAsync(int userId);
    Task<SupportTicketDto?> GetByIdAsync(int ticketId, int userId, bool isAdmin);
    Task<SupportTicketDto> CreateAsync(int userId, CreateTicketDto dto);
    Task<SupportTicketDto> ReplyAsync(int userId, ReplyToTicketDto dto, bool isStaff);
    Task<List<SupportTicketDto>> GetAllForAdminAsync();
    Task<SupportTicketDto> UpdateStatusAsync(int ticketId, string status);
    Task<SupportTicketDto> UpdatePriorityAsync(int ticketId, string priority);
    Task<TicketStatsDto> GetStatsAsync();
}