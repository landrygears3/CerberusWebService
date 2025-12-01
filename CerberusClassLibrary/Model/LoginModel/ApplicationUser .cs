using Microsoft.AspNetCore.Identity;

namespace CerberusClassLibrary.Model.LoginModel
{
    public class ApplicationUser : IdentityUser
    {
        public string NumeroUsuario { get; set; } = default!; 
        public bool IsActive { get; set; } = true; 
        public DateTime? FechaBaja { get; set; }
        public string? MotivoBaja { get; set; }
        public string? UsuarioBajaId { get; set; } 
    }
}
