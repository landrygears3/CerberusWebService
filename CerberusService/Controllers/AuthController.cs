using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model;
using CerberusClassLibrary.Model.LoginModel;
using CerberusClassLibrary.Model.LoginModel.DTO;
using CerberusClassLibrary.Model.LoginModel.JWT;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CerberusWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INumeroUsuarioService _numeroUsuarioService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            INumeroUsuarioService numeroUsuarioService)
        {
            _userManager = userManager;
            _numeroUsuarioService = numeroUsuarioService;
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
    }
}
