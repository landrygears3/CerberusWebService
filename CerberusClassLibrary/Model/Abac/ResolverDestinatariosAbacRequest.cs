using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.Abac
{
    public sealed class ResolverDestinatariosAbacRequest
    {
        public List<int> ActividadIds { get; set; } = new();

        public List<int> DepartamentoIds { get; set; } = new();

        public List<int> RolIds { get; set; } = new();

        /// <summary>
        /// ANY  = cumple cualquiera de los criterios.
        /// ALL  = debe cumplir todos los grupos de criterios enviados.
        /// </summary>
        public string MatchMode { get; set; } = "ANY";
    }
}
