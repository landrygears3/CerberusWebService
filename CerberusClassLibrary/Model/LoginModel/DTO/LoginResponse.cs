using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class LoginResponse
    {
        public string UserId { get; set; } = default!;
        public string NumeroUsuario { get; set; } = default!;
        public string Email { get; set; } = default!;

        public string AccessToken { get; set; } = default!;
        public DateTime AccessTokenExpiration { get; set; }

        public string RefreshToken { get; set; } = default!;
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
