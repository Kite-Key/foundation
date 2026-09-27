using System.Net.Mail;

namespace KiteKey.Mail;

public interface IMailSender : IDisposable
{
    bool CanSend { get; }
    MailMessage CreateMailMessage(string destinationAddresses, string subject, string body, bool isBodyHtml = false, string? fromAddress = null, string? fromDisplayName = null, string? replyToAddress = null)
    {
        MailMessage message = new() { Subject = subject, Body = body, IsBodyHtml = isBodyHtml };
        if (fromAddress is not null)
            message.From = new MailAddress(fromAddress, fromDisplayName);
        if (replyToAddress is not null)
            message.ReplyToList.Add(new MailAddress(replyToAddress));
        message.To.Add(destinationAddresses);
        return message;
    }
    Task<bool> SendAsync(MailMessage message, CancellationToken cancellationToken = default);
    Task<bool> SendSafeAsync(MailMessage message, CancellationToken cancellationToken = default);
    bool SendSync(MailMessage message);
    bool SendSafeSync(MailMessage message);
}
