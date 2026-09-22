using KHRMS.Core;
using KHRMS.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text.RegularExpressions;
using MailKit.Security;
using KHRMS.Core.Models;

public class SendEmailService : ISendEmailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public SendEmailService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<bool> SendTemplateEmailAsync(string toEmail, string subject, Dictionary<string, string> placeholders, string templateType)
    {
        try
        {
            var emailTemplate = await ResolveTemplateAsync(templateType);

            if (emailTemplate == null)
            {
                Console.WriteLine($"Active template for '{templateType}' not found or disabled.");
                return false;
            }

            // Replace placeholders dynamically using universal dual-syntax (#KEY# and {{KEY}}) with aliases
            string formattedBody = HydrateTemplate(emailTemplate.TemplateHtml, placeholders);

            var emailRequest = new Email
            {
                ToEmail = toEmail,
                EmailSubject = subject,
                EmailBody = formattedBody,
                EmailTemplateId = emailTemplate.Id
            };

            return await SendEmailAsync(emailRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendEventEmailAsync(string eventCode, string toEmail, Dictionary<string, string> placeholders, string? customSubject = null)
    {
        try
        {
            var emailTemplate = await ResolveTemplateAsync(eventCode);

            if (emailTemplate == null)
            {
                Console.WriteLine($"Trigger event '{eventCode}' is disabled or has no active template mapped.");
                return false;
            }

            string formattedBody = HydrateTemplate(emailTemplate.TemplateHtml, placeholders);
            string emailSubject = customSubject ?? $"KHRMS Notification - {eventCode}";

            var emailRequest = new Email
            {
                ToEmail = toEmail,
                EmailSubject = emailSubject,
                EmailBody = formattedBody,
                EmailTemplateId = emailTemplate.Id
            };

            return await SendEmailAsync(emailRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Event email sending failed for {eventCode}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string subject, string resetUrl, string templateType)
    {
        try
        {
            var emailTemplate = await ResolveTemplateAsync(templateType);

            if (emailTemplate == null)
            {
                Console.WriteLine($"Template '{templateType}' not found.");
                return false;
            }

            var dict = new Dictionary<string, string>
            {
                { "URL", resetUrl },
                { "ActionUrl", resetUrl },
                { "ResetUrl", resetUrl }
            };

            string formattedBody = HydrateTemplate(emailTemplate.TemplateHtml, dict);

            var emailRequest = new Email
            {
                ToEmail = toEmail,
                EmailSubject = subject,
                EmailBody = formattedBody,
                EmailTemplateId = emailTemplate.Id
            };

            return await SendEmailAsync(emailRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reset password email sending failed: {ex.Message}");
            return false;
        }
    }

    private async Task<EmailTemplatesMaster?> ResolveTemplateAsync(string eventCodeOrType)
    {
        // 1. Try resolving via EmailTriggerEvents table/registry
        try
        {
            var triggerEvents = await _unitOfWork.EmailTriggerEvents.GetAll();
            var matchedEvent = triggerEvents.FirstOrDefault(e =>
                string.Equals(e.EventCode, eventCodeOrType, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.EventName, eventCodeOrType, StringComparison.OrdinalIgnoreCase));

            if (matchedEvent != null)
            {
                if (!matchedEvent.IsEnabled)
                {
                    // Admin explicitly disabled this notification
                    return null;
                }

                if (matchedEvent.ActiveTemplateId.HasValue && matchedEvent.ActiveTemplateId.Value > 0)
                {
                    var boundTemplate = await _unitOfWork.EmailTemplateMaster.GetById(matchedEvent.ActiveTemplateId.Value);
                    if (boundTemplate != null) return boundTemplate;
                }
            }
        }
        catch
        {
            // Table might not be migrated yet - safe fallback
        }

        // 2. Try resolving via legacy EmailTemplateTypeMaster
        try
        {
            var allTypes = (await _unitOfWork.EmailTemplateTypeMaster.GetAll()).ToList();

            // Direct match
            var emailType = allTypes.FirstOrDefault(t => string.Equals(t.TemplateType, eventCodeOrType, StringComparison.OrdinalIgnoreCase));

            // Fuzzy match for leave requests (e.g. if code passed "Sick Leave" or "Casual Leave")
            if (emailType == null && eventCodeOrType.Contains("leave", StringComparison.OrdinalIgnoreCase))
            {
                emailType = allTypes.FirstOrDefault(t =>
                    t.TemplateType.Contains("leave", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.TemplateType, "ApprovalRequest", StringComparison.OrdinalIgnoreCase));
            }

            // Fallback for Onboarding Credentials
            if (emailType == null && (eventCodeOrType.Contains("onboard", StringComparison.OrdinalIgnoreCase) || eventCodeOrType.Contains("credential", StringComparison.OrdinalIgnoreCase)))
            {
                emailType = allTypes.FirstOrDefault(t =>
                    t.TemplateType.Contains("onboard", StringComparison.OrdinalIgnoreCase) ||
                    t.TemplateType.Contains("welcome", StringComparison.OrdinalIgnoreCase));
            }

            if (emailType != null)
            {
                var templates = await _unitOfWork.EmailTemplateMaster.GetAll();
                var template = templates.FirstOrDefault(t => t.EmailTemplateTypeId == emailType.Id);
                if (template != null) return template;
            }
        }
        catch
        {
        }

        // 3. Last fallback: return the first active template in the system if available
        try
        {
            var fallback = (await _unitOfWork.EmailTemplateMaster.GetAll()).FirstOrDefault();
            return fallback;
        }
        catch
        {
            return null;
        }
    }

    private static string HydrateTemplate(string templateHtml, Dictionary<string, string> placeholders)
    {
        if (string.IsNullOrEmpty(templateHtml)) return string.Empty;

        // Build expanded dictionary with automatic cross-compatibility aliases
        var expanded = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in placeholders)
        {
            expanded[kv.Key] = kv.Value ?? string.Empty;
        }

        // Auto-populate universal aliases so {{EmployeeName}}, #USERNAME#, {{ActionUrl}}, etc. all work!
        if (expanded.TryGetValue("USERNAME", out var username) && !expanded.ContainsKey("EmployeeName"))
            expanded["EmployeeName"] = username;
        if (expanded.TryGetValue("EmployeeName", out var empName) && !expanded.ContainsKey("USERNAME"))
            expanded["USERNAME"] = empName;

        if (expanded.TryGetValue("URL", out var url))
        {
            if (!expanded.ContainsKey("ActionUrl")) expanded["ActionUrl"] = url;
            if (!expanded.ContainsKey("PortalUrl")) expanded["PortalUrl"] = url;
            if (!expanded.ContainsKey("Link")) expanded["Link"] = url;
        }
        if (expanded.TryGetValue("ActionUrl", out var actUrl) && !expanded.ContainsKey("URL"))
            expanded["URL"] = actUrl;

        if (expanded.TryGetValue("COMPANY_NAME", out var comp) && !expanded.ContainsKey("CompanyName"))
            expanded["CompanyName"] = comp;
        if (expanded.TryGetValue("CompanyName", out var comp2) && !expanded.ContainsKey("COMPANY_NAME"))
            expanded["COMPANY_NAME"] = comp2;

        if (expanded.TryGetValue("LeaveDescription", out var ldesc) && !expanded.ContainsKey("Reason"))
            expanded["Reason"] = ldesc;
        if (expanded.TryGetValue("LeaveReason", out var lreas) && !expanded.ContainsKey("Reason"))
            expanded["Reason"] = lreas;
        if (expanded.TryGetValue("Reason", out var reas))
        {
            if (!expanded.ContainsKey("LeaveReason")) expanded["LeaveReason"] = reas;
            if (!expanded.ContainsKey("LeaveDescription")) expanded["LeaveDescription"] = reas;
        }

        if (expanded.TryGetValue("Resignation-Date", out var resDate) && !expanded.ContainsKey("ResignationDate"))
            expanded["ResignationDate"] = resDate;
        if (expanded.TryGetValue("ResignationDate", out var resDate2) && !expanded.ContainsKey("Resignation-Date"))
            expanded["Resignation-Date"] = resDate2;

        if (!expanded.ContainsKey("CurrentYear"))
            expanded["CurrentYear"] = DateTime.Now.Year.ToString();

        string result = templateHtml;

        // Replace both {{Key}} and #Key# (case-insensitive)
        foreach (var kv in expanded)
        {
            var escapedKey = Regex.Escape(kv.Key);
            // Matches #Key#, {{Key}}, or {Key}
            var pattern = $@"(\#|\{{{{?){escapedKey}(\#|\}}}}?)";
            result = Regex.Replace(result, pattern, kv.Value ?? string.Empty, RegexOptions.IgnoreCase);
        }

        return result;
    }
    public async Task<bool> SendEmailAsync(Email request)
    {
        try
        {
            // Create a new MimeMessage
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["SmtpSettings:SenderName"], _configuration["SmtpSettings:SenderEmail"]));  // "Sender Name" and "Sender Email"
            message.To.Add(new MailboxAddress(request.ToEmail, request.ToEmail));  // Recipient Email
            message.Subject = request.EmailSubject;

            // Create the body of the email (HTML content)
            var bodyBuilder = new BodyBuilder { HtmlBody = request.EmailBody };  // HTML Body
            message.Body = bodyBuilder.ToMessageBody();

            // Set up the SMTP client from MailKit
            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_configuration["SmtpSettings:Host"], int.Parse(_configuration["SmtpSettings:Port"]), SecureSocketOptions.StartTls); // Use StartTls for port 587
                await smtpClient.AuthenticateAsync(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]);
                await smtpClient.SendAsync(message);  // Send the email
                await smtpClient.DisconnectAsync(true);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email send failed: {ex.Message}");
            return false;
        }
    }

}