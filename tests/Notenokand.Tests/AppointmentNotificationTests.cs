using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notenokand.Domain.Entities;
using Notenokand.Domain.Enums;
using Notenokand.Web.Controllers;
using Notenokand.Web.Models.Notifications;

namespace Notenokand.Tests;

public sealed class AppointmentNotificationTests
{
    private static readonly DateTime Now = new(2026, 9, 17, 10, 0, 0);
    private static CalendarAppointment Appointment(int days, int? hour = null) => new()
    {
        AccountId = Guid.NewGuid(), OwnerUserId = Guid.NewGuid(), Title = "Test",
        Status = AppointmentStatus.Scheduled, ScheduledDate = DateOnly.FromDateTime(Now).AddDays(days),
        ScheduledTime = hour.HasValue ? new TimeOnly(hour.Value, 0) : null
    };

    [Theory]
    [InlineData(-1, null, true)]
    [InlineData(0, null, false)]
    [InlineData(0, 9, true)]
    [InlineData(0, 10, false)]
    [InlineData(0, 11, false)]
    [InlineData(1, 9, false)]
    public void OverdueHonorsThaiLocalDateAndOptionalTime(int days, int? hour, bool expected)
    {
        Assert.Equal(expected, NotificationViewModel.IsOverdue(Appointment(days, hour), Now));
    }

    [Fact]
    public void SummaryGroupsAreMutuallyExclusive()
    {
        var model = new NotificationViewModel
        {
            LocalNow = Now,
            Appointments = [Appointment(-1), Appointment(0, 9), Appointment(0), Appointment(0, 11), Appointment(1), Appointment(7)]
        };
        Assert.Equal(2, model.OverdueCount);
        Assert.Equal(2, model.TodayCount);
        Assert.Equal(2, model.UpcomingCount);
        Assert.Equal(model.Appointments.Count, model.OverdueCount + model.TodayCount + model.UpcomingCount);
    }

    [Fact]
    public void UndatedTimeBecomesOverdueAtNextMidnight()
    {
        var item = Appointment(0);
        Assert.False(NotificationViewModel.IsOverdue(item, new DateTime(2026, 9, 17, 23, 59, 59)));
        Assert.True(NotificationViewModel.IsOverdue(item, new DateTime(2026, 9, 18)));
    }

    [Fact]
    public void CompletionRequiresLoginPostAndAntiForgery()
    {
        var controller = typeof(NotificationsController);
        Assert.NotEmpty(controller.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        var method = controller.GetMethod(nameof(NotificationsController.Complete))!;
        Assert.NotEmpty(method.GetCustomAttributes(typeof(HttpPostAttribute), true));
        Assert.NotEmpty(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true));
    }
}
