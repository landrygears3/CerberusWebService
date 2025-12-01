using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class RegisterResponse
    {
        public string UserId { get; set; } = default!;
        public string NumeroUsuario { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
