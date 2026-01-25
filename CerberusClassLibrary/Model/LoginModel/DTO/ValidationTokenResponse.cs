using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class ValidationTokenResponse
    {
        public string Message { get; set; } = default!;
        public bool IsValid { get; set; } = false;
    }
}
