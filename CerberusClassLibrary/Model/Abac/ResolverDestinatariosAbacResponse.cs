using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.Abac
{
    public sealed class ResolverDestinatariosAbacResponse
    {
        public string NumeroUsuario { get; set; } = null!;

        public List<int> ActividadIds { get; set; } = new();

        public List<int> DepartamentoIds { get; set; } = new();

        public List<int> RolIds { get; set; } = new();
    }
}
