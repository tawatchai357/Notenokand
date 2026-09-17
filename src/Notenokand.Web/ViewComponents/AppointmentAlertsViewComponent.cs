using Microsoft.AspNetCore.Mvc;
using Notenokand.Web.Services;
namespace Notenokand.Web.ViewComponents;

public sealed class AppointmentAlertsViewComponent(AppointmentNotificationService notifications) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync() =>
        View(await notifications.LoadAsync(HttpContext.User));
}
