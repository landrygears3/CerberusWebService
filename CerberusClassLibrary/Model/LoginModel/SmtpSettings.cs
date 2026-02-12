using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string From { get; set; } = default!;
    }
}
