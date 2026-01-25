using Azure.Core;
using CerberusClassLibrary.Controller.LoginController;
using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model.LoginModel;
using CerberusClassLibrary.Model.LoginModel.DTO;
using CerberusClassLibrary.Model.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using System.Net;
using System.Reflection;
using System.Text;

namespace CerberusWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly EmailTemplateEngine _emailTemplateEngine;
        public PasswordController(EmailTemplateEngine emailTemplateEngine,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IEmailService emailService)
        {
            _userManager = userManager;
            _env = env;
            _emailService = emailService;
            _emailTemplateEngine = emailTemplateEngine;
        }

        // POST: api/password/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                    throw new Exception("Correo es obligatorio.");

                var genericMessage = "Si existe una cuenta asociada al correo, se ha enviado un enlace para restablecer la contraseña.";

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // No revelar existencia
                    return Ok(new ForgotPasswordResponse { Message = genericMessage });
                }

                // Generar token y codificar seguro para URL
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var tokenBytes = Encoding.UTF8.GetBytes(token);
                var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

                var origin = string.IsNullOrWhiteSpace(request.ClientUrl)
                    ? $"{Request.Scheme}://{Request.Host}"
                    : request.ClientUrl!.TrimEnd('/');

                var resetUrl = $"{origin}/reset-password?email={WebUtility.UrlEncode(request.Email)}&token={encodedToken}";
                //Formato de correo
                EmailConfigs formatemail = _emailTemplateEngine.getEmailConfig("ForgotPassword", request.Email);

                // Enviar correo
                string html = await EmailTemplateEngine.LoadTemplateAsync(formatemail.PhisicalPath);
                Dictionary<string,string> templatekeys = new Dictionary<string, string>();
                templatekeys["Nombre"] = formatemail.Name;
                templatekeys["ConfirmUrl"] = resetUrl;
                templatekeys["Year"] = DateTime.Now.Year.ToString();

                html = EmailTemplateEngine.Render(html, templatekeys);
                SendMailModel mailConfig = new SendMailModel
                {
                    subject = "Restablecer contraseña",
                    toEmail = request.Email,
                    htmlMessage = html

                };               

                await _emailService.SendAsync(mailConfig);

                // En desarrollo, opcionalmente devolver la URL para pruebas internas
                if (_env.IsDevelopment())
                {
                    return Ok(new ForgotPasswordResponse { Message = genericMessage, ResetUrl = resetUrl });
                }

                return Ok(new ForgotPasswordResponse { Message = genericMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            
        }

        // POST: api/password/reset-password
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateResetToken([FromBody] ValidationTokenRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new ValidationTokenResponse { Message = "Email y token son obligatorios." });
            }
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // No revelar existencia; devolver mensaje genérico
                return BadRequest(new ValidationTokenResponse { Message = "Token inválido." });
            }
            // Decodificar token Base64Url
            string token;
            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
                token = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                return BadRequest(new ValidationTokenResponse { Message = "Token inválido." });
            }
            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                token);
            if (!isValid)
            {
                return BadRequest(new ValidationTokenResponse { Message = "Token inválido." });
            }
            return Ok(new ValidationTokenResponse { Message = "Token válido." ,IsValid = true});
        }

        // POST: api/password/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Email, token y nueva contraseña son obligatorios." });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // No revelar existencia; devolver mensaje genérico
                return BadRequest(new ResetPasswordResponse { Message = "No se pudo restablecer la contraseña." });
            }

            // Decodificar token Base64Url
            string token;
            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
                token = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                return BadRequest(new ResetPasswordResponse { Message = "Token inválido." });
            }

            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new ResetPasswordResponse { Message = "Error al restablecer la contraseña.", Errors = errors });
            }

            return Ok(new ResetPasswordResponse { Message = "Contraseña restablecida correctamente." });
        }
    }
}
