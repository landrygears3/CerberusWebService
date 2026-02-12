using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = default!;
        // Token codificado en Base64Url
        public string Token { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
