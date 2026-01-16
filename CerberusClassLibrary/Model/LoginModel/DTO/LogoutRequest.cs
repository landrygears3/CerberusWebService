using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel.DTO
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = default!;
        public bool LogoutAllDevices { get; set; } = false; 
    }
}
