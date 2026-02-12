using Azure.Core;
using CerberusClassLibrary.Controller.LoginController;
using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model;
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
        public async Task<ResponseModel<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            ResponseModel<ForgotPasswordResponse> response = new ResponseModel<ForgotPasswordResponse>();
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                
                    response.IsSuccess = false;
                    response.Message = "El correo electrónico es obligatorio.";
                }
                else
                {
                    var genericMessage = "Si existe una cuenta asociada al correo, se ha enviado un enlace para restablecer la contraseña.";

                    var user = await _userManager.FindByEmailAsync(request.Email);
                    if (user == null)
                    {
                        response.IsSuccess = true;
                        response.Message = genericMessage;
                        response.Code = 200;
                        response.Data = null;
                    }
                    else
                    {
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
                        Dictionary<string, string> templatekeys = new Dictionary<string, string>();
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

                        response.IsSuccess = true;
                        response.Message = genericMessage;
                        response.Code = 200;
                        response.Data = null;
                        // En desarrollo, opcionalmente devolver la URL para pruebas internas
                        if (_env.IsDevelopment())
                        {
                            response.Data = new ForgotPasswordResponse { ResetUrl = resetUrl };
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Error al intentar recuperar contraseña";
                response.Code = 500;
                response.Data = null;
                response.Desc = ex.Message;
            }
            return response;
        }

        // POST: api/password/reset-password
        [HttpPost("validate-token")]
        public async Task<ResponseModel<ValidationTokenResponse>> ValidateResetToken([FromBody] ValidationTokenRequest request)
        {
            ResponseModel<ValidationTokenResponse> response = new ResponseModel<ValidationTokenResponse>();
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token))
            {
                response.IsSuccess = false;
                response.Message = "Email y token son obligatorios.";
                response.Code = 500;
                response.Data = new ValidationTokenResponse { IsValid = false };
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // No revelar existencia; devolver mensaje genérico
                    response.IsSuccess = false;
                    response.Message = "Token inválido.";
                    response.Code = 500;
                    response.Data = new ValidationTokenResponse { IsValid = false };
                }
                else
                {
                    // Decodificar token Base64Url
                    string token = string.Empty;
                    try
                    {
                        var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
                        token = Encoding.UTF8.GetString(tokenBytes);
                    }
                    catch
                    {
                        response.IsSuccess = false;
                        response.Message = "Token inválido.";
                        response.Code = 500;
                        response.Data = new ValidationTokenResponse { IsValid = false };
                    }
                    if (!string.IsNullOrEmpty(token))
                    {
                        var isValid = await _userManager.VerifyUserTokenAsync(
                        user,
                        _userManager.Options.Tokens.PasswordResetTokenProvider,
                        "ResetPassword",
                        token);
                        if (!isValid)
                        {
                            response.IsSuccess = false;
                            response.Message = "Token inválido.";
                            response.Code = 500;
                            response.Data = new ValidationTokenResponse { IsValid = false };
                        }
                        else
                        {
                            response.IsSuccess = true;
                            response.Message = "Token válido.";
                            response.Code = 200;
                            response.Data = new ValidationTokenResponse { IsValid = true };
                        }
                        
                    }
                    
                }
                
            }
            return response;
        }

        // POST: api/password/reset-password
        [HttpPost("reset-password")]
        public async Task<ResponseModel<ResetPasswordResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            ResponseModel<ResetPasswordResponse> response = new ResponseModel<ResetPasswordResponse>();
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                response.Data = null;
                response.IsSuccess = false;
                response.Message = "Email, token y nueva contraseña son obligatorios.";
                response.Code = 400;
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // No revelar existencia; devolver mensaje genérico
                    response.Code = 400;
                    response.Message = "No se pudo restablecer la contraseña.";
                    response.Data = null;
                    response.IsSuccess = false;
                }
                else
                {
                    // Decodificar token Base64Url
                    string token = string.Empty;
                    try
                    {
                        var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
                        token = Encoding.UTF8.GetString(tokenBytes);
                    }
                    catch
                    {
                        response.IsSuccess = false;
                        response.Message = "Token inválido.";
                        response.Code = 500;
                        response.Data = null;
                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                        if (!result.Succeeded)
                        {
                            var errors = result.Errors.Select(e => e.Description);
                            response.IsSuccess = false;
                            response.Message = "Error al restablecer la contraseña.";
                            response.Data = new ResetPasswordResponse { Errors = errors };
                            response.Code = 400;
                        }else
                        {
                            response.IsSuccess = true;
                            response.Code = 200;
                            response.Message = "Contraseña restablecida correctamente.";
                            response.Data = null;
                        }
                        
                    }
                    else
                    {
                        // No revelar existencia; devolver mensaje genérico
                        response.Code = 400;
                        response.Message = "No se pudo obtener token";
                        response.Data = null;
                        response.IsSuccess = false;
                        
                    }
                   
                }

                
            }

           return response;
        }
    }
}
