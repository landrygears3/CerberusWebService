using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class RevokeTokenRequest
    {
        public string RefreshToken { get; set; } = default!;
    }

}
