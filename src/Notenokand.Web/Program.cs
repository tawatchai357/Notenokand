using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Notenokand.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Notenokand.Web.ModelBinding;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddScoped<Notenokand.Web.Services.AccountPermissionFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new IsoTemporalModelBinderProvider());
    options.Filters.AddService<Notenokand.Web.Services.AccountPermissionFilter>();
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".data-protection")))
    .SetApplicationName("Notenokand");
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<Notenokand.Web.Services.BuildingPhotoStorage>();
builder.Services.AddScoped<Notenokand.Web.Services.AppointmentNotificationService>();
builder.Services.AddScoped<Notenokand.Web.Services.LotSaleService>();
builder.Services.AddScoped<Notenokand.Web.Services.AccountEmailService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.OnStarting(() =>
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>() is not null)
            context.Response.Headers.CacheControl = "no-store";
        return Task.CompletedTask;
    });
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
