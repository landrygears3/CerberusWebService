using CerberusClassLibrary.Controller.LoginController;
using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model;
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
        public async Task<ResponseModel<string>> EnviaMailBienvenida([FromBody] MailBienvenidaRequest request)
        {
            ResponseModel<string> response = new ResponseModel<string>();
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
                response.IsSuccess = true;
                response.Message = "Correo de bienvenida enviado exitosamente.";
                response.Code = 200;
            }
            catch (Exception ex)
            {
                response.Message = "Error al enviar el correo de bienvenida.";
                response.IsSuccess = false;
                response.Desc = ex.Message;
                response.Code = 500;

            }

            return response;
        }
    }
}
