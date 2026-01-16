using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class ForgotPasswordResponse
    {
        // Mensaje genérico para no revelar existencia de cuenta.
        public string Message { get; set; } = default!;
        // Solo en desarrollo o pruebas: URL de restablecimiento (no usar en producción).
        public string? ResetUrl { get; set; }
    }
}
