using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.Mail
{
    public class SendMailModel
    {
        public string subject { get; set; }
        public string toEmail { get; set; }
        public string? inlineLogoPath { get; set; } = @"CerberusClassLibrary.Resources.Images.logoB.png";
        public string htmlMessage { get; set; }
    }
}
