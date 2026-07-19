namespace BabyShop.Application.Dtos;

public class DashboardStatsDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int TodayNewTickets { get; set; }
    public int WeeklyNewTickets { get; set; }
}