using FitnessTracker.Application.Interfaces;
using FitnessTracker.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace FitnessTracker.Infrastructure.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailSender(IOptions<SmtpSettings> smtpSettingsOptions)
        {
            _smtpSettings = smtpSettingsOptions.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_smtpSettings.Host) ||
                _smtpSettings.Port == 0 ||
                string.IsNullOrWhiteSpace(_smtpSettings.Username) ||
                string.IsNullOrWhiteSpace(_smtpSettings.Password) ||
                string.IsNullOrWhiteSpace(_smtpSettings.From))
            {
                return;
            }

            try
            {
                var senderDisplayName = !string.IsNullOrWhiteSpace(_smtpSettings.SenderDisplayName)
                                        ? _smtpSettings.SenderDisplayName
                                        : _smtpSettings.From;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.From, senderDisplayName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                using (var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
                {
                    smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (SmtpException)
            {
                throw; 
            }
            catch (System.Exception) 
            {
                throw;
            }
        }
    }
}