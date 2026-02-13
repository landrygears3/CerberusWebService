using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.Abac
{
    public class SavePersonaAccessRequest
    {
        public string UserNumber { get; set; } = default!; // CER00010
        public int? DepartamentoId { get; set; }
        public List<int> RoleIds { get; set; } = new();
        public List<PersonaRuleItem> PersonaRules { get; set; } = new(); // bool -> allow/deny
    }
}
