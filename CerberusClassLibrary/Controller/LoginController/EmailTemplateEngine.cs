using CerberusClassLibrary.DataSecure;
using CerberusClassLibrary.Model.Mail;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CerberusClassLibrary.Controller.LoginController
{
    public class EmailTemplateEngine
    {
        private readonly string _cn;

        public EmailTemplateEngine(ConnectionStringProvider connectionStringProvider)
        {
            _cn = connectionStringProvider.ConnectionString;
        }
        public static async Task<string> LoadTemplateAsync(string templatePath)
        {
            // templatePath ej: "EmailTemplates/Welcome/template.html"
            var assembly = Assembly.GetExecutingAssembly();
            using var stram = assembly.GetManifestResourceStream(templatePath.Replace("/", ".").Replace("\\", "."));
            using var reader = new StreamReader(stram);
            return reader.ReadToEnd();
        }

        public static string Render(string html, IDictionary<string, string> tokens)
        {
            if (tokens == null || tokens.Count == 0) return html;

            foreach (var kv in tokens)
            {
                // placeholders estilo: {{Nombre}}
                html = html.Replace("{{" + kv.Key + "}}", kv.Value ?? "", StringComparison.Ordinal);
            }
            return html;
        }

        public EmailConfigs getEmailConfig(string module,string email)
        {
            try
            {
                EmailConfigs ret = new EmailConfigs();
                SqlConnection conn = new SqlConnection(_cn);
                string query = "SELECT * FROM EmailTemplates WHERE MODULE = '" + module + "'";
                SqlCommand commad = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = commad.ExecuteReader();


                while (reader.Read())
                {
                    ret.Module = reader.GetString(0);
                    ret.PhisicalPath = reader.GetString(1);
                }
                conn.Close();
                query = "SELECT * FROM EmailParams WHERE MODULE = '" + module + "'";
                commad = new SqlCommand(query, conn);
                conn.Open();
                reader = commad.ExecuteReader();
                while (reader.Read())
                {
                    ret.Tokens.Add(reader.GetString(1));
                }
                conn.Close();
                conn.Close();
                query = "SELECT NumeroUsuario FROM AspNetUsers WHERE Email = '" + email + "'";
                commad = new SqlCommand(query, conn);
                conn.Open();
                reader = commad.ExecuteReader();
                while (reader.Read())
                {
                    ret.Name = reader.GetString(0);
                }
                conn.Close();
                return ret;
            }
            catch(Exception ex)
            {
                throw new Exception("Error al obtener la configuración de email: " + ex.Message);
            }
            
        }
    }
}
