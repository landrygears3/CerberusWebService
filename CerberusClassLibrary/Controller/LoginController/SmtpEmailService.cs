using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model.LoginModel;
using CerberusClassLibrary.Model.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Reflection;
using System.Text;


namespace CerberusClassLibrary.Controller.LoginController
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailService(SmtpSettings settings)
        {
            _settings = settings;
        }

        public async Task SendAsync(SendMailModel mailConfig)
        {

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_settings.From));
            message.To.Add(MailboxAddress.Parse(mailConfig.toEmail));
            message.Subject = mailConfig.subject;
            var builder = new BodyBuilder
            {
                HtmlBody = mailConfig.htmlMessage,
                TextBody = "Este correo requiere un cliente compatible con HTML." // fallback
            };

            //Filesream logo
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(mailConfig.inlineLogoPath);
            if (stream != null)
            {
                var logo = builder.LinkedResources.Add("logoB.png", stream);
                logo.ContentId = "logo";
                logo.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            }

            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
