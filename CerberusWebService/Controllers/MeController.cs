using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace CerberusWebService.Controllers
{
    [ApiController]
    [Route("api/me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly string _cs;

        public MeController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")!;
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("No se encontró el id del usuario en el token.");

            var allowed = new List<string>();

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand("sp_GetAllowedActivities", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@AspNetUserId", SqlDbType.NVarChar, 450)
            {
                Value = userId
            });

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                allowed.Add(reader.GetString(0));
            }

            return Ok(new { allowedActivities = allowed });
        }
    }
}
