using System;
using System.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailSender
{
    public void SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            // Đọc cấu hình SMTP từ web.config
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            var smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            var smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
            var enableSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"]);

            // Tạo message
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(smtpUser));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using (var client = new SmtpClient())
            {
                // Kết nối SMTP với SSL hoặc StartTLS
                client.Connect(smtpHost, smtpPort, SecureSocketOptions.StartTls);

                // Xác thực SMTP
                client.Authenticate(smtpUser, smtpPass);

                // Gửi mail
                client.Send(message);

                client.Disconnect(true);
            }

            Console.WriteLine("Gửi mail thành công tới " + toEmail);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Lỗi gửi mail: " + ex.Message);
            throw;
        }
    }
}
