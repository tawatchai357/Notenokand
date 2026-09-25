using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace Notenokand.Web.Services;

public sealed class AccountEmailService(IConfiguration configuration, ILogger<AccountEmailService> logger)
{
    private IConfigurationSection Settings => configuration.GetSection("Email");
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Settings["Host"]) &&
        !string.IsNullOrWhiteSpace(Settings["From"]) && BaseUri() is not null;
    private Uri? BaseUri() => Uri.TryCreate(configuration["PublicBaseUrl"], UriKind.Absolute, out var uri) &&
        uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) &&
        string.IsNullOrEmpty(uri.Fragment) ? uri : null;
    public string Link(string relativePath) =>
        new Uri(new Uri((BaseUri() ?? throw new InvalidOperationException("Configure PublicBaseUrl with HTTPS.")).AbsoluteUri.TrimEnd('/') + "/"), relativePath.TrimStart('/')).AbsoluteUri;

    public async Task<bool> SendLinkAsync(string email, string subject, string link)
    {
        if (!IsConfigured) return false;
        try
        {
            using var message = new MailMessage(Settings["From"]!, email)
            {
                Subject = subject, IsBodyHtml = true,
                Body = $"<p>{HtmlEncoder.Default.Encode(subject)}</p><p><a href=\"{HtmlEncoder.Default.Encode(link)}\">ดำเนินการต่อใน Notenokand</a></p><p>หากคุณไม่ได้ขอรายการนี้ ให้ละเว้นอีเมลฉบับนี้</p>"
            };
            using var client = new SmtpClient(Settings["Host"], Settings.GetValue("Port", 587))
            {
                EnableSsl = true, UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Settings["Username"], Settings["Password"]),
                Timeout = 15000
            };
            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception ex) when (ex is SmtpException or FormatException or InvalidOperationException)
        {
            logger.LogWarning("Account email delivery failed ({FailureType}). No token or address logged.", ex.GetType().Name);
            return false;
        }
    }
}
