using CerberusClassLibrary.Model;
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
        public async Task<ResponseModel<List<string>>> GetPermissions(CancellationToken ct)
        {
            ResponseModel<List<string>> response = new ResponseModel<List<string>>();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
            {
                response.IsSuccess = false;
                response.Code = 401;
                response.Message = "No se encontró el id del usuario en el token.";
                response.Desc = "Unauthorized";
                response.Data = null;
                return response;
            }
            else
            {
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
                response.IsSuccess = true;
                response.Code = 200;
                response.Message = "Permisos obtenidos correctamente.";
                response.Data = allowed;
            }                      

            return response;
        }
    }
}
