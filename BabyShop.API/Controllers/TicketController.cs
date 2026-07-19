using BabyShop.Application.Dtos;
using BabyShop.Application.Interfaces.Services;
using BabyShop.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BabyShop.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ISupportTicketService _ticketService;
    private readonly ILogger<TicketController> _logger;

    public TicketController(ISupportTicketService ticketService, ILogger<TicketController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id) && id > 0) return id;
        throw new UnauthorizedAccessException("User id missing");
    }

    private bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("Manager");

    [HttpGet]
    public async Task<IActionResult> GetMyTickets()
    {
        try
        {
            var list = await _ticketService.GetMyTicketsAsync(GetUserId());
            return Ok(new ApiResponse<List<SupportTicketDto>> { Success = true, Data = list });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMyTickets failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var ticket = await _ticketService.GetByIdAsync(id, GetUserId(), IsAdmin);
            if (ticket == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Ticket not found" });
            return Ok(new ApiResponse<SupportTicketDto> { Success = true, Data = ticket });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(403, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        try
        {
            var ticket = await _ticketService.CreateAsync(GetUserId(), dto);
            return Ok(new ApiResponse<SupportTicketDto>
            {
                Success = true,
                Data = ticket,
                Message = "Ticket created"
            });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create ticket failed");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("reply")]
    public async Task<IActionResult> Reply([FromBody] ReplyToTicketDto dto)
    {
        try
        {
            var ticket = await _ticketService.ReplyAsync(GetUserId(), dto, IsAdmin);
            return Ok(new ApiResponse<SupportTicketDto>
            {
                Success = true,
                Data = ticket,
                Message = "Reply added"
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
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }
}