using CerberusClassLibrary.Model.LoginModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Interfaz
{
    public interface ITokenService
    {
        Task<(string token, DateTime expires)> CreateAccessTokenAsync(ApplicationUser user);

        Task<(string token, DateTime expires)> CreateRefreshTokenAsync(
            ApplicationUser user,
            string ipAddress);
    }
}
