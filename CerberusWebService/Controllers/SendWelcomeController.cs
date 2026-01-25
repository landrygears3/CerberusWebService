using CerberusClassLibrary.Controller.LoginController;
using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model.Mail;
using Microsoft.AspNetCore.Mvc;

namespace CerberusWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SendWelcomeController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly EmailTemplateEngine _emailTemplateEngine;

        public SendWelcomeController(EmailTemplateEngine emailTemplateEngine, IEmailService emailService)
        {
            _emailService = emailService;
            _emailTemplateEngine = emailTemplateEngine;
        }

        [HttpPost("enviar-bienvenida")]
        public async Task<IActionResult> EnviaMailBienvenida([FromBody] MailBienvenidaRequest request)
        {
            try
            {
                EmailConfigs formatemail = _emailTemplateEngine.getEmailConfig("Welcome", request.ToEmail);
                string html = await EmailTemplateEngine.LoadTemplateAsync(formatemail.PhisicalPath);
                Dictionary<string, string> templatekeys = new Dictionary<string, string>();
                templatekeys["usuario"] = formatemail.Name;
                templatekeys["contrasena"] = request.Password;
                templatekeys["Year"] = DateTime.Now.Year.ToString();
                html = EmailTemplateEngine.Render(html, templatekeys);
                SendMailModel mailConfig = new SendMailModel
                {
                    subject = "CSP - Bienvenido a Cerberus",
                    toEmail = request.ToEmail,
                    htmlMessage = html

                };

                await _emailService.SendAsync(mailConfig);
                return Ok("Correo de bienvenida enviado exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al enviar el correo de bienvenida: {ex.Message}");

            }
        }
    }
}
