using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model.LoginModel;
using CerberusClassLibrary.Model.LoginModel.DTO;
using CerberusClassLibrary.Model.LoginModel.JWT;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validación básica
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email y contraseña son obligatorios." });
            }

            // Revisar si el correo ya existe
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
            {
                return BadRequest(new { message = "El correo ya está registrado." });
            }

            // Generar NumeroUsuario: CER00001, CER00002, ...
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
                return BadRequest(new
                {
                    message = "Error al crear el usuario.",
                    errors
                });
            }

            var response = new RegisterResponse
            {
                UserId = user.Id,
                NumeroUsuario = user.NumeroUsuario,
                Email = user.Email!
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserNameOrNumero) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Usuario y contraseña son obligatorios." });
            }

            // Buscar usuario por email o numeroUsuario
            ApplicationUser? user;

            if (request.UserNameOrNumero.Contains("@"))
                user = await _userManager.FindByEmailAsync(request.UserNameOrNumero);
            else
                user = await _userManager.Users
                       .FirstOrDefaultAsync(x => x.NumeroUsuario == request.UserNameOrNumero);

            if (user == null)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

            // Checar baja lógica
            if (!user.IsActive)
                return Unauthorized(new { message = "La cuenta está inactiva." });

            // Validar contraseña
            var valid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!valid)
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

            // IP del cliente
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";


            // Access token
            (string accessToken, DateTime accessExpires) =
                await _tokenService.CreateAccessTokenAsync(user);

            // Refresh token
            (string refreshToken, DateTime refreshExpires) =
                await _tokenService.CreateRefreshTokenAsync(user, ip);


            var response = new LoginResponse
            {
                UserId = user.Id,
                NumeroUsuario = user.NumeroUsuario,
                Email = user.Email!,
                AccessToken = accessToken,
                AccessTokenExpiration = accessExpires,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshExpires
            };

            return Ok(response);
        }

    }
}
