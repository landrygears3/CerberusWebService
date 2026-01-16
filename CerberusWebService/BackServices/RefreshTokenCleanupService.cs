using CerberusClassLibrary.DataSecure;
using Microsoft.EntityFrameworkCore;

namespace CerberusWebService.BackServices
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Corre cada 12 horas (ajústalo)
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<CerberusDbContext>();

                    var now = DateTime.UtcNow;

                    // Opción 1: borrar tokens expirados hace más de 7 días (ejemplo)
                    var cutoff = now.AddDays(-7);

                    var toDelete = await db.UserRefreshTokens
                        .Where(t => t.ExpiresAt < cutoff)
                        .ToListAsync(stoppingToken);

                    if (toDelete.Count > 0)
                    {
                        db.UserRefreshTokens.RemoveRange(toDelete);
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("RefreshTokenCleanup: eliminados {Count}", toDelete.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RefreshTokenCleanup falló");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }

        }
    }
}
