using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Entities;
using BabyShop.Core.Exceptions;
using BabyShop.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BabyShop.Application.Services
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _repo;
        private readonly IUserRepository _userRepo; // اضافه شد برای خوندن اسم کاربر
        private readonly ILogger<SupportTicketService> _logger;

        public SupportTicketService(ISupportTicketRepository repo, IUserRepository userRepo, ILogger<SupportTicketService> logger)
        {
            _repo = repo;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<List<SupportTicketDto>> GetMyTicketsAsync(int userId)
        {
            var tickets = await _repo.GetByUserIdAsync(userId);
            var result = new List<SupportTicketDto>();
            foreach (var t in tickets)
                result.Add(await MapAsync(t));
            return result.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public async Task<SupportTicketDto?> GetByIdAsync(int ticketId, int userId, bool isAdmin)
        {
            var ticket = await _repo.GetByIdAsync(ticketId);
            if (ticket == null) return null;
            if (!isAdmin && ticket.UserId != userId)
                throw new BusinessRuleException("شما به این تیکت دسترسی ندارید");
            return await MapAsync(ticket);
        }

        public async Task<SupportTicketDto> CreateAsync(int userId, CreateTicketDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Subject))
                throw new BusinessRuleException("موضوع تیکت الزامی است");
            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new BusinessRuleException("متن پیام الزامی است");

            var ticket = new SupportTicket
            {
                TicketNumber = await _repo.GenerateTicketNumberAsync(),
                UserId = userId,
                Subject = dto.Subject.Trim(),
                Message = dto.Message.Trim(),
                Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim(),
                Status = "Open",
                Priority = "Medium",
                OrderId = dto.OrderId is > 0 ? dto.OrderId : null,
                ProductId = dto.ProductId is > 0 ? dto.ProductId : null,
                CreatedAt = DateTime.UtcNow
            };

            ticket = await _repo.AddAsync(ticket);

            await _repo.AddReplyAsync(new TicketReply
            {
                TicketId = ticket.Id,
                UserId = userId,
                Message = dto.Message.Trim(),
                IsAdminReply = false,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Ticket {TicketId} created by user {UserId}", ticket.Id, userId);
            return (await GetByIdAsync(ticket.Id, userId, true))!;
        }

        public async Task<SupportTicketDto> ReplyAsync(int userId, ReplyToTicketDto dto, bool isStaff)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new BusinessRuleException("متن پاسخ الزامی است");

            var ticket = await _repo.GetByIdAsync(dto.TicketId)
                ?? throw new NotFoundException("Ticket", dto.TicketId);

            if (!isStaff && ticket.UserId != userId)
                throw new BusinessRuleException("شما به این تیکت دسترسی ندارید");

            await _repo.AddReplyAsync(new TicketReply
            {
                TicketId = ticket.Id,
                UserId = userId,
                Message = dto.Message.Trim(),
                IsAdminReply = isStaff,
                CreatedAt = DateTime.UtcNow
            });

            // وقتی ادمین جواب می‌ده، وضعیت از Open درمیاد
            if (isStaff && ticket.Status == "Open")
            {
                ticket.Status = "InProgress";
                ticket.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(ticket);
            }

            return (await GetByIdAsync(ticket.Id, userId, isStaff || ticket.UserId == userId))!;
        }

        public async Task<List<SupportTicketDto>> GetAllForAdminAsync()
        {
            var tickets = await _repo.GetAllAsync();
            var result = new List<SupportTicketDto>();
            foreach (var t in tickets)
                result.Add(await MapAsync(t));
            return result.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public async Task<SupportTicketDto> UpdateStatusAsync(int ticketId, string status)
        {
            var ticket = await _repo.GetByIdAsync(ticketId)
                ?? throw new NotFoundException("Ticket", ticketId);

            var allowed = new[] { "Open", "InProgress", "Resolved", "Closed" };
            if (!allowed.Contains(status, StringComparer.OrdinalIgnoreCase))
                throw new BusinessRuleException("Status must be: Open | InProgress | Resolved | Closed");

            ticket.Status = allowed.First(a => a.Equals(status, StringComparison.OrdinalIgnoreCase));
            ticket.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(ticket);
            return await MapAsync(ticket);
        }

        public async Task<SupportTicketDto> UpdatePriorityAsync(int ticketId, string priority)
        {
            var ticket = await _repo.GetByIdAsync(ticketId)
                ?? throw new NotFoundException("Ticket", ticketId);

            var allowed = new[] { "Low", "Medium", "High", "Urgent", "Normal" };
            if (!allowed.Contains(priority, StringComparer.OrdinalIgnoreCase))
                throw new BusinessRuleException("Priority must be: Low | Medium | High | Urgent");

            ticket.Priority = allowed.First(a => a.Equals(priority, StringComparison.OrdinalIgnoreCase));
            ticket.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(ticket);
            return await MapAsync(ticket);
        }

        public async Task<TicketStatsDto> GetStatsAsync()
        {
            return new TicketStatsDto
            {
                Total = await _repo.CountAllAsync(),
                Open = await _repo.CountByStatusAsync("Open"),
                InProgress = await _repo.CountByStatusAsync("InProgress"),
                Resolved = await _repo.CountByStatusAsync("Resolved"),
                Closed = await _repo.CountByStatusAsync("Closed")
            };
        }

        private async Task<SupportTicketDto> MapAsync(SupportTicket ticket)
        {
            var replies = await _repo.GetRepliesAsync(ticket.Id);

            // اینجا دیتای واقعی کاربر رو از دیتابیس می‌خونیم
            var user = await _userRepo.GetByIdAsync(ticket.UserId);

            return new SupportTicketDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Subject = ticket.Subject,
                Message = ticket.Message,
                UserId = ticket.UserId,

                // اطلاعات واقعی به فرانت‌اند ارسال می‌شه
                UserName = user?.FullName ?? "کاربر ناشناس",
                UserPhone = user?.PhoneNumber ?? "-",

                OrderId = ticket.OrderId,
                ProductId = ticket.ProductId,
                ProductName = string.Empty,
                Status = ticket.Status,
                Priority = ticket.Priority,
                Category = ticket.Category,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                Replies = replies.Select(r => new TicketReplyDto
                {
                    Id = r.Id,
                    TicketId = r.TicketId,
                    UserId = r.UserId,
                    UserName = r.IsAdminReply ? "پشتیبانی زی‌زی" : (user?.FullName ?? "کاربر"),
                    Message = r.Message,
                    IsAdminReply = r.IsAdminReply,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };
        }
    }
}