using InventorySales.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System;

namespace InventorySales.Application.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var host = _configuration["MailSettings:Host"];
            var port = int.Parse(_configuration["MailSettings:Port"] ?? "587");
            var senderMail = _configuration["MailSettings:Mail"];
            var password = _configuration["MailSettings:Password"];

            using var smtpClient = new SmtpClient(host);
            smtpClient.Port = port;
            smtpClient.Credentials = new NetworkCredential(senderMail, password);
            smtpClient.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderMail!, "Inventory System"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}