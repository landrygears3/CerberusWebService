using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model;
using CerberusClassLibrary.Model.LoginModel;
using CerberusClassLibrary.Model.LoginModel.DTO;
using CerberusClassLibrary.Model.LoginModel.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CerberusWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INumeroUsuarioService _numeroUsuarioService;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            INumeroUsuarioService numeroUsuarioService,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _numeroUsuarioService = numeroUsuarioService;
            _tokenService = tokenService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ResponseModel<RegisterResponse>> Register([FromBody] CerberusClassLibrary.Model.LoginModel.DTO.RegisterRequest request)
        {

            ResponseModel<RegisterResponse> response = new ResponseModel<RegisterResponse>();
            // Validación básica
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                response.Code = 400;
                response.IsSuccess = false;
                response.Message = "Email y contraseña son obligatorios.";
            }
            else
            {

                // Revisar si el correo ya existe
                var existing = await _userManager.FindByEmailAsync(request.Email);
                if (existing != null)
                {
                    response.Code = 400;
                    response.IsSuccess = false;
                    response.Message = "El correo ya está registrado.";
                }
                else
                {
                    var numeroUsuario = await _numeroUsuarioService.GenerateNextAsync();

                    var user = new ApplicationUser
                    {
                        UserName = request.Email,
                        Email = request.Email,
                        PhoneNumber = request.Telefono,
                        NumeroUsuario = numeroUsuario,
                        IsActive = true,
                        FechaBaja = null,
                        MotivoBaja = null,
                        UsuarioBajaId = null
                    };

                    // Crear usuario con Identity (hash de password incluido)
                    var result = await _userManager.CreateAsync(user, request.Password);

                    if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description);
                        response.Code = 400;
                        response.IsSuccess = false;
                        response.Message = "Error al crear el usuario.";
                        response.Desc = string.Join("; ", errors);
                    }
                    else
                    {
                        response.Code = 200;
                        response.Message = "Usuario dado de alta de manera correcta";
                        response.IsSuccess = true;
                        response.Data = new RegisterResponse
                        {
                            UserId = user.Id,
                            NumeroUsuario = user.NumeroUsuario,
                            Email = user.Email!
                        };
                    }
                       
                }
                    // Generar NumeroUsuario: CER00001, CER00002, ...
               
            }

                return response;
        }

        [HttpPost("login")]
        public async Task<ResponseModel<LoginResponse>> Login([FromBody] CerberusClassLibrary.Model.LoginModel.DTO.LoginRequest request)
        {
            ResponseModel<LoginResponse> response = new ResponseModel<LoginResponse>();
            if (string.IsNullOrWhiteSpace(request.UserNameOrNumero) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                response.Message = "Usuario y contraseña son obligatorios.";
                response.IsSuccess = false;
                response.Code = 400;
            }

            // Buscar usuario por email o numeroUsuario
            ApplicationUser? user;

            if (request.UserNameOrNumero.Contains("@"))
                user = await _userManager.FindByEmailAsync(request.UserNameOrNumero);
            else
                user = await _userManager.Users
                       .FirstOrDefaultAsync(x => x.NumeroUsuario == request.UserNameOrNumero);

            if (user == null)
            {
                response.Message = "Usuario o contraseña incorrectos.";
                response.IsSuccess = false;
                response.Code = 401;
            }
            else
            {


                // Checar baja lógica
                if (!user.IsActive)
                {
                    response.IsSuccess = false;
                    response.Message = "La cuenta está inactiva.";
                    response.Code = 401;
                }
                else
                {
                    // Validar contraseña
                    var valid = await _userManager.CheckPasswordAsync(user, request.Password);
                    if (!valid)
                    {
                        response.IsSuccess = false;
                        response.Message = "Usuario o contraseña incorrectos.";
                        response.Code = 401;
                    }
                    else
                    {

                        // IP del cliente
                        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";


                        // Access token
                        (string accessToken, DateTime accessExpires) =
                            await _tokenService.CreateAccessTokenAsync(user);

                        // Refresh token
                        (string refreshToken, DateTime refreshExpires) =
                            await _tokenService.CreateRefreshTokenAsync(user, ip);


                        response.Data = new LoginResponse
                        {
                            UserId = user.Id,
                            NumeroUsuario = user.NumeroUsuario,
                            Email = user.Email!,
                            AccessToken = accessToken,
                            AccessTokenExpiration = accessExpires,
                            RefreshToken = refreshToken,
                            RefreshTokenExpiration = refreshExpires
                        };
                        response.IsSuccess = true;
                        response.Code = 200;
                        response.Message = "Inicio de sesión exitoso.";
                    }
                }
            }
                return response;
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ResponseModel<string>> Logout([FromBody] LogoutRequest request)
        {
            ResponseModel<string> response = new ResponseModel<string>();
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                response.Code = 400;
                response.IsSuccess = false;
                response.Message = "RefreshToken es obligatorio.";
            }
            else
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                if (request.LogoutAllDevices)
                {
                    // userId viene del JWT (sub)
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                 ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        response.Code = 401;
                        response.IsSuccess = false;
                        response.Message = "No autorizado.";
                    }
                    else
                    {
                        await _tokenService.RevokeAllRefreshTokensAsync(userId, ip);
                    }
                        
                }
                else
                {
                    await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, ip);
                }

                response.Message = "Sesión cerrada.";
                response.IsSuccess = true;
                response.Code = 200;

            }


            return response;
        }

        [HttpPost("refresh")]
        public async Task<ResponseModel<RefreshTokenResponse>> Refresh([FromBody] RefreshRequest request)
        {
            ResponseModel<RefreshTokenResponse> response = new ResponseModel<RefreshTokenResponse>();
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                response.Message = "RefreshToken es obligatorio.";
                response.IsSuccess = false;
                response.Code = 400;
            }
            else
            {
                try
                {
                    var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    var (user, newRefreshToken, newRefreshExpires) =
                        await _tokenService.RotateRefreshTokenAsync(request.RefreshToken, ip);

                    var (accessToken, accessExpires) =
                        await _tokenService.CreateAccessTokenAsync(user);

                    response.Data = new RefreshTokenResponse
                    {
                        AccessToken = accessToken,
                        AccessTokenExpiration = accessExpires,
                        RefreshToken = newRefreshToken,
                        RefreshTokenExpiration = newRefreshExpires
                    };
                    response.IsSuccess = true;
                    response.Code = 200;
                    response.Message = "Token renovado correctamente.";
                }
                catch (InvalidOperationException ex)
                {
                    // Importante: no des demasiados detalles si no quieres.
                    response.Message = "No fue posible actualizar el token";
                    response.Code = 401;
                    response.Desc = ex.Message;
                    response.IsSuccess = false;
                }
            }

               

            return response;
        }

    }
}
