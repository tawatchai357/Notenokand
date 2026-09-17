using Notenokand.Domain.Entities;
namespace Notenokand.Web.Models.Notifications;

public sealed class NotificationViewModel
{
    public DateTime LocalNow { get; init; }
    public IReadOnlyList<CalendarAppointment> Appointments { get; init; } = [];
    public int OverdueCount => Appointments.Count(x => IsOverdue(x, LocalNow));
    public int TodayCount => Appointments.Count(x => !IsOverdue(x, LocalNow) && x.ScheduledDate == DateOnly.FromDateTime(LocalNow));
    public int UpcomingCount => Appointments.Count - OverdueCount - TodayCount;
    public static bool IsOverdue(CalendarAppointment item, DateTime localNow) =>
        item.ScheduledDate < DateOnly.FromDateTime(localNow) ||
        (item.ScheduledDate == DateOnly.FromDateTime(localNow) && item.ScheduledTime.HasValue &&
         item.ScheduledTime.Value < TimeOnly.FromDateTime(localNow));
    public string Group(CalendarAppointment item) => IsOverdue(item, LocalNow) ? "overdue" :
        item.ScheduledDate == DateOnly.FromDateTime(LocalNow) ? "today" : "upcoming";
}
