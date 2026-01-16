using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.Mail
{
    public class EmailConfigs
    {
        public string Module { get; set; } = default!;

        public string PhisicalPath { get; set; } = default!;

        public string Name { get; set; } = default!;

        public List<string> Tokens { get; set; } = new List<string>();
    }
}
