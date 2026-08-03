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
            // Fetch the email template from the database based on the provided templateType
             var emailType = (await _unitOfWork.EmailTemplateTypeMaster.GetAll()).FirstOrDefault(t => t.TemplateType == templateType);
            var emailDescription = emailType?.Description;
            var emailTemplate = (await _unitOfWork.EmailTemplateMaster.GetAll()).FirstOrDefault(t => t.EmailTemplateTypeId == emailType.Id);

            if (emailTemplate == null)
            {
                Console.WriteLine($"Template '{templateType}' not found.");
                return false;
            }

            // Replace placeholders dynamically in the email body
            string formattedBody = emailTemplate.TemplateHtml;

            foreach (var placeholder in placeholders)
            {
                formattedBody = Regex.Replace(formattedBody, $"#{placeholder.Key}#", placeholder.Value, RegexOptions.IgnoreCase);
            }

            // Create the email request
            var emailRequest = new Email
            {
                ToEmail = toEmail,
                EmailSubject = subject,
                EmailBody = formattedBody,  // This is the formatted body with replaced placeholders
                EmailTemplateId = emailTemplate.Id
            };

            // Call the existing SendEmailAsync method to actually send the email
            return await SendEmailAsync(emailRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string subject, string placeholders, string templateType)
    {
        try
        {
            // Fetch the email template from the database based on the provided templateType
            var emailType = (await _unitOfWork.EmailTemplateTypeMaster.GetAll()).FirstOrDefault(t => t.TemplateType == templateType);
            var emailDescription = emailType?.Description;
            var emailTemplate = (await _unitOfWork.EmailTemplateMaster.GetAll()).FirstOrDefault(t => t.EmailTemplateTypeId == emailType.Id);

            if (emailTemplate == null)
            {
                Console.WriteLine($"Template '{templateType}' not found.");
                return false;
            }

            // Replace placeholders dynamically in the email body
            string formattedBody = emailTemplate.TemplateHtml;

            formattedBody = Regex.Replace(formattedBody, "#URL#", placeholders, RegexOptions.IgnoreCase);

            // Create the email request
            var emailRequest = new Email
            {
                ToEmail = toEmail,
                EmailSubject = subject,
                EmailBody = formattedBody,  // This is the formatted body with replaced placeholders
                EmailTemplateId = emailTemplate.Id
            };

            // Call the existing SendEmailAsync method to actually send the email
            return await SendEmailAsync(emailRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            return false;
        }
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