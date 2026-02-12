using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = default!;
        // Opcional: URL base del cliente (ej. https://mi-front.example) para construir el enlace de restablecimiento.
        public string? ClientUrl { get; set; }
    }
}
