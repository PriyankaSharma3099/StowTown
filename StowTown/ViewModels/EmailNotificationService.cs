using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using StowTown.Models;
using StowTown.ViewModels;

public class EmailNotificationService
{
    private readonly StowTownDbContext _context;

    public EmailNotificationService(StowTownDbContext context)
    {
        _context = context;
    }

    private string smtpServer = "smtp.gmail.com"; // Replace with your SMTP server
    private int smtpPort = 587; // Typically 587 for TLS, 465 for SSL
    private string senderEmail = UserSession.UserName; // Replace with your email
    private string senderPassword = UserSession.EmailPassword; // Replace with your email password

    private void SendEmailWithAttachment(List<(string Email, string Name)> activeStations)
    {
        var fromAddress = new MailAddress(senderEmail);
        var subject = "Monthly Spin Count Request";

        using (var smtp = new SmtpClient
        {
            Host = smtpServer,
            Port = smtpPort,
            EnableSsl = true,
            Credentials = new NetworkCredential(senderEmail, senderPassword)
        })
        {
            foreach (var station in activeStations)
            {
                using (var message = new MailMessage())
                {
                    message.From = fromAddress;
                    message.To.Add(station.Email);
                    message.Subject = subject;
                    message.Body = GenerateEmailBody(station); // Generate body for each station
                    smtp.Send(message);
                }
            }
        }
    }

    public async Task SendMonthlyEmailsAsync()
    {
        try
        {
            var activeStations = await GetActiveRadioStations();
            if (activeStations.Count == 0)
            {
               // MessageBox.Show("No email addresses found.");
                return;
            }

            // Send emails to each active station
            SendEmailWithAttachment(activeStations);

           // MessageBox.Show("Emails sent successfully!");
        }
        catch (Exception ex)
        {
            // Handle exception (logging, etc.)
            throw new Exception("Error sending emails: " + ex.Message);
        }
    }

    private async Task<List<(string Email, string Name)>> GetActiveRadioStations()
    {
        // Fetch emails and names from the RadioStation table
        var stations = await _context.RadioStations
            .Where(rs => rs.IsActive==true && rs.IsDeleted != true && !string.IsNullOrEmpty(rs.Email)) // Ensure IsActive is true and Email is not null or empty
            .Select(rs => new { rs.Email, rs.Name }) // Create an anonymous object
            .ToListAsync();

        // Convert to a list of tuples
        var emailList = stations.Select(station => (station.Email, station.Name)).ToList();

        return emailList;
    }


    private string GenerateEmailBody((string Email, string Name) station)
    {
        var bodyBuilder = new StringBuilder();
        bodyBuilder.AppendLine($"Dear {station.Name},");
        bodyBuilder.AppendLine();
        bodyBuilder.AppendLine("I hope this message finds you well.");
        bodyBuilder.AppendLine();
        bodyBuilder.AppendLine("As we approach the end of the month, we kindly request your assistance in providing the spin count and position for all the songs.");
        bodyBuilder.AppendLine();
        bodyBuilder.AppendLine("Please reply to this email with the requested details by the end of the week. Your cooperation is greatly appreciated.");
        bodyBuilder.AppendLine();
        bodyBuilder.AppendLine("Thank you for your attention to this matter.");
        bodyBuilder.AppendLine();
        bodyBuilder.AppendLine("Best regards,");
        bodyBuilder.AppendLine("StowTown Team");

        return bodyBuilder.ToString();
    }
}
