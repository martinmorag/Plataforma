using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace Plataforma.Servicios
{
    public class EmailSend : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSend(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
            {
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                client.EnableSsl = true; // Use SSL/TLS encryption
                client.UseDefaultCredentials = false;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true // Set to true if your message contains HTML
                };
                mailMessage.To.Add(toEmail);

                try
                {
                    await client.SendMailAsync(mailMessage);
                    // If you're here, it succeeded.
                }
                catch (SmtpException smtpEx)
                {
                    // More specific SmtpException handling
                    throw new SmtpException($"SMTP Error sending email: {smtpEx.Message}. Server response: {smtpEx.StatusCode} {smtpEx.Message}", smtpEx);
                }
                catch (Exception ex)
                {
                    // Generic exception for other network/system issues
                    throw new Exception($"General error sending email: {ex.Message}", ex);
                }
            }
        }
    }
}
