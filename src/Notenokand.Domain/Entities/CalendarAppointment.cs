using Notenokand.Domain.Common;
using Notenokand.Domain.Enums;

namespace Notenokand.Domain.Entities;

public sealed class CalendarAppointment : Entity
{
    public Guid AccountId { get; set; }
    public required Guid OwnerUserId { get; set; }
    public Guid? BuildingId { get; set; }
    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public DateOnly ScheduledDate { get; set; }
    public TimeOnly? ScheduledTime { get; set; }
    public required string Title { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public BirdBuilding? Building { get; set; }
}