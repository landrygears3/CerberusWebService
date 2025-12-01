using CerberusClassLibrary.DataSecure;
using CerberusClassLibrary.Interfaz;          // 👈 importante
using CerberusClassLibrary.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace CerberusClassLibrary.Model.LoginModel.JWT
{
    public class TokenService : ITokenService   // <-- esta ITokenService es la de Interfaz
    {
        private readonly JwtSettings _jwtSettings;
        private readonly CerberusDbContext _db;

        public TokenService(JwtSettings jwtOptions, CerberusDbContext db)
        {
            _jwtSettings = jwtOptions;
            _db = db;
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
    }
}
