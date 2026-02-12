using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class ValidationTokenRequest
    {
        public string Token { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
