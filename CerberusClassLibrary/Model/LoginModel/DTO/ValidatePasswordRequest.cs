using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class ValidatePasswordRequest
    {
        public string NumeroUsuario { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
