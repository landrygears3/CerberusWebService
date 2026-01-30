using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class ForgotPasswordResponse
    {

        // Solo en desarrollo o pruebas: URL de restablecimiento (no usar en producción).
        public string? ResetUrl { get; set; }
    }
}
