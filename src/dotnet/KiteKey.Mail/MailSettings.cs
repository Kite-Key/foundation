using System.Net.Mail;

namespace KiteKey.Mail;

/// <summary>SMTP connection settings; supply credentials through the consuming application's configuration.</summary>
public sealed class MailSettings
{
    public string? FromAddress { get; set; }
    public string? FromDisplayName { get; set; }
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = true;

    public bool Validate()
        => !string.IsNullOrWhiteSpace(SmtpServer)
            && SmtpPort is > 0 and <= 65535
            && MailAddress.TryCreate(FromAddress, out _)
            && (string.IsNullOrEmpty(UserName) == string.IsNullOrEmpty(Password));
}
