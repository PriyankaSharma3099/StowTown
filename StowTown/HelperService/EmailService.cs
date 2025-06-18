using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StowTown.HelperService
{
    public class EmailService
    {

        private readonly string _smtpServer = "smtp.gmail.com";  // Change as needed
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "priyanka.sharma.microlent@gmail.com";
        private readonly string _smtpPassword = "jsvv hzsw hkto qaja";  // Use App Password for security

        /// <summary>
        /// Opens the default email client with an attachment
        /// </summary>
        public async Task OpenEmailClient(string recipient, string subject, string body, string filePath = null)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    To = new List<string> { recipient }
                };

                // Attach file if it exists
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    message.Attachments.Add(new EmailAttachment(filePath));
                }

                await Email.Default.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Sends an email using SMTP with an attachment
        /// </summary>
        public async Task SendEmailViaSMTP(string recipient, string subject, string body, string filePath = null)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Your App", _smtpUser));
                email.To.Add(new MailboxAddress("Recipient", recipient));
                email.Subject = subject;

                var textBody = new TextPart("plain") { Text = body };

                var multipart = new Multipart("mixed") { textBody };

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    var attachment = new MimePart("application", "octet-stream")
                    {
                        Content = new MimeContent(File.OpenRead(filePath), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(filePath)
                    };
                    multipart.Add(attachment);
                }

                email.Body = multipart;

                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
                    await smtpClient.SendAsync(email);
                    await smtpClient.DisconnectAsync(true);
                }

                //await Application.Current.MainPage.DisplayAlert("Success", "Email Sent Successfully!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }


        public async Task<bool> SendPasswordEmail(string recipientEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                return false;

            if (string.IsNullOrWhiteSpace(password))
                return false;

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_smtpUser));
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject = "Your Generated New Password";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"<p>Your generated  password is: <strong>{password}</strong></p>",
                TextBody = $"Your generated  new password is: {password}"
            };
            email.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_smtpUser, _smtpPassword);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);

                Console.WriteLine($"Email successfully sent to {recipientEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email to {recipientEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
