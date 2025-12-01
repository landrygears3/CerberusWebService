using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Interfaz
{
    public interface INumeroUsuarioService
    {
        Task<string> GenerateNextAsync();
    }
}
