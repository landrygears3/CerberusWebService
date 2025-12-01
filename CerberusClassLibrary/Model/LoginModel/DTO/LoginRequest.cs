using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class LoginRequest
    {
        // Puede ser email o numeroUsuario (CER00001)
        public string UserNameOrNumero { get; set; } = default!;

        public string Password { get; set; } = default!;

        //marca "Recordarme"
        public bool RememberMe { get; set; } = false;
    }

}
