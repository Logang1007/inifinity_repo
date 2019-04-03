using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;

namespace Infinity.Automation.Lib.Helpers
{
    public class EmailHelper: IEmailHelper
    {
        public string Smtp { get; set; }
        public int Port { get; set; }


        public EmailHelper(string smtp, int port)
        {
            Smtp = smtp;
            Port = port;
        }

        public void Send(string fromEmailAddress, List<string> toEmailAddress, string subject, string mailMessage, List<EmailAttachment> emailAttachment = null)
        {
            string smtp = Smtp;
            int port = Port;

            SmtpClient client = new SmtpClient(Smtp, Port);

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmailAddress);
            mail.IsBodyHtml = true;
            foreach (var item in toEmailAddress)
            {
                mail.To.Add(item);
            }

            mail.Body = mailMessage;
            mail.Subject = subject;
            if (emailAttachment != null)
            {
                foreach (var item in emailAttachment)
                {
                    string fileName = Guid.NewGuid().ToString();
                    if (!string.IsNullOrEmpty(item.FileName))
                    {
                        fileName = item.FileName;
                    }

                    Attachment attachment = new Attachment(new MemoryStream(item.File), fileName);
                    mail.Attachments.Add(attachment);
                }
            }
            client.Send(mail);
        }

        private bool _customCertificateValidationCallback(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public async Task SendAsync(string fromEmailAddress, List<string> toEmailAddress, string subject, string mailMessage, List<EmailAttachment> emailAttachment = null)
        {

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromEmailAddress);
            foreach (var item in toEmailAddress)
            {
                mail.To.Add(item);
            }
            mail.IsBodyHtml = true;
            mail.Body = mailMessage;
            mail.Subject = subject;

            if (emailAttachment != null)
            {
                foreach (var item in emailAttachment)
                {
                    string fileName = Guid.NewGuid().ToString();
                    if (!string.IsNullOrEmpty(item.FileName))
                    {
                        fileName = item.FileName;
                    }

                    Attachment attachment = new Attachment(new MemoryStream(item.File), fileName);
                    mail.Attachments.Add(attachment);
                }
            }


            using (var smtpClient = new SmtpClient(Smtp, Port))
            {
                await smtpClient.SendMailAsync(mail);
            }

        }
    }

  
}
