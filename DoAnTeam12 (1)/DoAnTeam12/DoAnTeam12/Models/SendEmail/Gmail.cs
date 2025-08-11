using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Configuration;

namespace DoAnTeam12.Models.SendEmail
{
    public class Gmail
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public void SendEmail()
        {
            try
            {
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                bool smtpEnableSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpEnableSsl"]);

                MailMessage mc = new MailMessage(smtpUser, To);
                mc.Subject = Subject;
                mc.Body = Body;
                mc.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.Timeout = 10000;
                smtp.EnableSsl = smtpEnableSsl;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                NetworkCredential nc = new NetworkCredential(smtpUser, smtpPass);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = nc;

                smtp.Send(mc);
                Console.WriteLine("Email sent successfully to " + To);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine("SMTP Error: " + ex.StatusCode);
                Console.WriteLine("Message: " + ex.Message);
                throw new Exception("Failed to send email. Please check your SMTP settings and credentials.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Error: " + ex.Message);
                throw new Exception("An unexpected error occurred while sending email.", ex);
            }
        }
    }
}
