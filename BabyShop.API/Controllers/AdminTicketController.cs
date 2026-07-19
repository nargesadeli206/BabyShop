using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabyShop.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/AdminTicket")]
public class AdminTicketController : ControllerBase
{
    private readonly ISupportTicketService _ticketService;

    public AdminTicketController(ISupportTicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _ticketService.GetAllForAdminAsync();
        return Ok(new ApiResponse<List<SupportTicketDto>> { Success = true, Data = list });
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var list = await _ticketService.GetAllForAdminAsync();
        var filtered = list
            .Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Ok(new ApiResponse<List<SupportTicketDto>> { Success = true, Data = filtered });
    }

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> Stats()
    {
        var stats = await _ticketService.GetStatsAsync();
        return Ok(new ApiResponse<TicketStatsDto> { Success = true, Data = stats });
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateTicketStatusDto dto)
    {
        try
        {
            var ticket = await _ticketService.UpdateStatusAsync(dto.TicketId, dto.Status);
            return Ok(new ApiResponse<SupportTicketDto>
            {
                Success = true,
                Data = ticket,
                Message = "Status updated"
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("priority")]
    public async Task<IActionResult> UpdatePriority([FromBody] UpdateTicketPriorityDto dto)
    {
        try
        {
            var ticket = await _ticketService.UpdatePriorityAsync(dto.TicketId, dto.Priority);
            return Ok(new ApiResponse<SupportTicketDto>
            {
                Success = true,
                Data = ticket,
                Message = "Priority updated"
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }
}