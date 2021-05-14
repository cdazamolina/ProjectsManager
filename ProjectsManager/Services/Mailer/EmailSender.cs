using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;

namespace ProjectsManager.Services.Mailer
{
    public class EmailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        private readonly string _mailTemplate = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/templates/mail_template.html"
        );
        public EmailSender(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public Task SendEmailWithFilesAsync(string email, string subject, string message, string[] filesPaths)
        {
            return Execute(subject, message, email, filesPaths);
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(subject, message, email);
        }

        public Task Execute(string subject, string text, string email, string[] filesPaths = null)
        {
            MimeMessage message = new(){ Subject = subject};
            message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            message.To.Add(new MailboxAddress("Custom", email));

            BodyBuilder bodyBuilder = new(){ HtmlBody = text };

            if (filesPaths != null)
                foreach (string file in filesPaths)
                    bodyBuilder.Attachments.Add(file.Replace("\\","/"));

            foreach (var attachement in bodyBuilder.Attachments)
                attachement.ContentDisposition.FileName = $"Reporte_{bodyBuilder.Attachments.IndexOf(attachement)}.csv";
            
            message.Body = bodyBuilder.ToMessageBody();

            SmtpClient client = new()
            {
                CheckCertificateRevocation = true,
                ServerCertificateValidationCallback = MySslCertificateValidationCallback
            };

            client.Connect(_mailSettings.Host, _mailSettings.Port);
            client.Authenticate(_mailSettings.Mail, _mailSettings.Password);

            return client.SendAsync(message);
        }

        //No se valida nada relacionado con los certificados SSL de destino.
        static bool MySslCertificateValidationCallback(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public string HTMLTemplate(string text)
        {
            StreamReader str = new StreamReader(_mailTemplate);
            string MailText = str.ReadToEnd();
            str.Close();
            MailText = MailText.Replace("[Text]", text);
            return MailText;
        }
    }
}
