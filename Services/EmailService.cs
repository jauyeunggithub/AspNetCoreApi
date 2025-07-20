using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    public void SendEmail(string toEmail, string subject, string body)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("NoReply", "noreply@example.com"));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder { TextBody = body };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            client.Connect("smtp.example.com", 587, false);
            client.Authenticate("your_email@example.com", "your_password");
            client.Send(emailMessage);
            client.Disconnect(true);
        }
    }
}
