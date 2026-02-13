using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.Abac
{
    public class PersonaAccessResponse
    {
        public int DepartamentoId { get; set; }
        public List<int> RoleIds { get; set; } = new();
        public List<PersonaRuleResponseItem> PersonaRules { get; set; } = new();
    }
}
