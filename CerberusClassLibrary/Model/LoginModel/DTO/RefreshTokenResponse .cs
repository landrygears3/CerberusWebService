using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class RefreshTokenResponse : LoginResponse
    {
        public string AccessToken { get; set; } = default!;
        public DateTime AccessTokenExpiration { get; set; }

        public string RefreshToken { get; set; } = default!;
        public DateTime RefreshTokenExpiration { get; set; }
    }

}
