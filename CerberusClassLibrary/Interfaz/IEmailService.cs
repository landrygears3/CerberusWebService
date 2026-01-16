using CerberusClassLibrary.Model.Mail;
using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Interfaz
{
    public interface IEmailService
    {
        Task SendAsync(SendMailModel mailConfig);
    }
}
