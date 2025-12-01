using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model.LoginModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Controller.LoginController
{
    public class NumeroUsuarioService : INumeroUsuarioService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public NumeroUsuarioService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GenerateNextAsync()
        {
            const string prefix = "CER";

            var last = await _userManager.Users
                .Where(u => u.NumeroUsuario.StartsWith(prefix))
                .OrderByDescending(u => u.NumeroUsuario)
                .FirstOrDefaultAsync();

            if (last == null)
                return $"{prefix}00001";

            var numericPart = last.NumeroUsuario.Substring(prefix.Length); // "00001"
            if (!int.TryParse(numericPart, out var number))
                number = 0;

            number++;
            return $"{prefix}{number:00000}";
        }
    }
}
