using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class ResetPasswordResponse
    {
        public string Message { get; set; } = default!;
        public IEnumerable<string>? Errors { get; set; }
    }
}
