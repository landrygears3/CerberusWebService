using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Model.LoginModel
{
    public class UserRefreshToken
    {
        public int Id { get; set; }

        // FK al usuario de Identity
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        // El token en sí (puede ser un GUID largo o un string generado)
        public string Token { get; set; } = default!;

        // Vigencia del refresh token
        public DateTime ExpiresAt { get; set; }

        // Estado del token
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Datos de auditoría (opcional pero útil)
        public string? CreatedByIp { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }

        // Si este token fue reemplazado por otro (rotación de tokens)
        public string? ReplacedByToken { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
