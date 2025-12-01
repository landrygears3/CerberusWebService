                 
using CerberusClassLibrary.Model.LoginModel;      
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CerberusClassLibrary.DataSecure  
{
    public class CerberusDbContext : IdentityDbContext<ApplicationUser>
    {
        public CerberusDbContext(DbContextOptions<CerberusDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = default!;
    }
}
