using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.Abac
{
    public class PersonaRuleResponseItem
    {
        public int ActividadId { get; set; }
        public bool IsAllowed { get; set; } // true=Allow, false=Deny
    }
}
