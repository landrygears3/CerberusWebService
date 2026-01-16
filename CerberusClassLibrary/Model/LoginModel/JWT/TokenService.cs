using CerberusClassLibrary.DataSecure;
using CerberusClassLibrary.Interfaz;          // 👈 importante
using CerberusClassLibrary.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace CerberusClassLibrary.Model.LoginModel.JWT
{
    public class TokenService : ITokenService   
    {
        private readonly JwtSettings _jwtSettings;
        private readonly CerberusDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(JwtSettings jwtSettings, CerberusDbContext db, UserManager<ApplicationUser> userManager)
        {
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public Task<(string token, DateTime expires)> CreateAccessTokenAsync(ApplicationUser user)
        {
            Console.WriteLine(">>> Entrando a TokenService.CreateAccessTokenAsync"); // debug

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_jwtSettings.AccessTokenMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim("num", user.NumeroUsuario),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
            return Task.FromResult((tokenString, expires));
        }

        public async Task<(string token, DateTime expires)> CreateRefreshTokenAsync(
            ApplicationUser user,
            string ipAddress)
        {
            Console.WriteLine(">>> Entrando a TokenService.CreateRefreshTokenAsync"); // debug

            var expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var token = Convert.ToBase64String(randomBytes);

            var refresh = new UserRefreshToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = expires,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                IsActive = true
            };

            _db.UserRefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            return (token, expires);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var tokenRow = await _db.UserRefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (tokenRow == null)
                return; // no revelamos si existe o no

            tokenRow.IsActive = false;

            await _db.SaveChangesAsync();
        }

        public async Task RevokeAllRefreshTokensAsync(string userId, string ipAddress)
        {
            var tokens = await _db.UserRefreshTokens
                .Where(x => x.UserId == userId && x.IsActive)
                .ToListAsync();

            foreach (var t in tokens)
                t.IsActive = false;

            await _db.SaveChangesAsync();
        }
        public async Task<(ApplicationUser user, string newRefreshToken, DateTime newRefreshExpires)>
    RotateRefreshTokenAsync(string refreshToken, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidOperationException("Refresh token vacío.");

            var now = DateTime.UtcNow;

            var existing = await _db.UserRefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (existing == null)
                throw new InvalidOperationException("Refresh token inválido.");

            if (!existing.IsActive)
                throw new InvalidOperationException("Refresh token inactivo.");

            if (existing.ExpiresAt <= now)
                throw new InvalidOperationException("Refresh token expirado.");

            // Cargar usuario
            var user = await _userManager.FindByIdAsync(existing.UserId);
            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            // Baja lógica (si ya la agregaste a ApplicationUser)
            if (!user.IsActive)
                throw new InvalidOperationException("La cuenta está dada de baja.");

            // ROTACIÓN: desactivar el token anterior
            existing.IsActive = false;

            // Crear uno nuevo (reusa tu método ya existente)
            var (newToken, newExpires) = await CreateRefreshTokenAsync(user, ipAddress);

            // Persistir el cambio de IsActive=false del anterior
            await _db.SaveChangesAsync();

            return (user, newToken, newExpires);
        }



    }
}
