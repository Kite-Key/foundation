using System.Net;
using System.Net.Mail;
using KiteKey.Core.Concurrency;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KiteKey.Mail;

/// <summary>SMTP sender. An optional recipient allowlist prevents non-production sends to arbitrary addresses.</summary>
public sealed class MailSender : IMailSender
{
    private readonly MailSettings _settings;
    private readonly HashSet<string>? _allowedDestinations;
    private readonly ILogger<MailSender> _logger;
    private readonly AsyncLock _lock = new();
    public bool CanSend { get; }

    public MailSender(MailSettings settings, IEnumerable<string>? allowedDestinations = null, ILogger<MailSender>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        CanSend = settings.Validate();
        _allowedDestinations = allowedDestinations is null ? null : new HashSet<string>(allowedDestinations, StringComparer.OrdinalIgnoreCase);
        _logger = logger ?? NullLogger<MailSender>.Instance;
    }

    public MailMessage CreateMailMessage(string destinationAddresses, string subject, string body, bool isBodyHtml = false, string? fromAddress = null, string? fromDisplayName = null, string? replyToAddress = null)
    {
        MailMessage message = new() { Subject = subject, Body = body, IsBodyHtml = isBodyHtml };
        if (fromAddress is not null)
            message.From = new MailAddress(fromAddress, fromDisplayName);
        if (replyToAddress is not null)
            message.ReplyToList.Add(new MailAddress(replyToAddress));
        message.To.Add(destinationAddresses);
        return message;
    }

    public async Task<bool> SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!CanSend || !Allowed(message))
            return false;
        message.From ??= new MailAddress(_settings.FromAddress!, _settings.FromDisplayName);
        using AsyncLock.Handle handle = await _lock.WaitAsync(cancellationToken);
        using SmtpClient client = CreateClient();
        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Sent mail {Subject} to {Address}", message.Subject, message.To.ToString());
        return true;
    }

    public bool SendSync(MailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!CanSend || !Allowed(message))
            return false;
        message.From ??= new MailAddress(_settings.FromAddress!, _settings.FromDisplayName);
        using AsyncLock.Handle handle = _lock.WaitSync();
        using SmtpClient client = CreateClient();
        client.Send(message);
        _logger.LogInformation("Sent mail {Subject} to {Address}", message.Subject, message.To.ToString());
        return true;
    }

    public async Task<bool> SendSafeAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        try { return await SendAsync(message, cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send mail");
            return false;
        }
    }

    public bool SendSafeSync(MailMessage message)
    {
        try { return SendSync(message); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send mail");
            return false;
        }
    }

    public void Dispose() => _lock.Dispose();

    private bool Allowed(MailMessage message)
    {
        if (_allowedDestinations is null || (message.To.Count > 0
            && message.To.Cast<MailAddress>().Concat(message.CC.Cast<MailAddress>()).Concat(message.Bcc.Cast<MailAddress>())
                .All(address => _allowedDestinations.Contains(address.Address))))
            return true;
        _logger.LogWarning("Mail {Subject} rejected by recipient allowlist", message.Subject);
        return false;
    }

    private SmtpClient CreateClient()
        => new(_settings.SmtpServer!, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = string.IsNullOrEmpty(_settings.UserName) ? null : new NetworkCredential(_settings.UserName, _settings.Password)
        };
}
