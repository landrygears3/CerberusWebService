using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.Mail
{
    public class MailBienvenidaRequest
    {
        public string ToEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
