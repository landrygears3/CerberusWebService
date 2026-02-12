using CerberusClassLibrary.Model;
using CerberusClassLibrary.Model.LoginModel.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CerberusWebService.Controllers
{
    [ApiController]
    [Route("api/abac")]
    [Authorize]
    public class AbacController : ControllerBase
    {
        private readonly string _cs;

        public AbacController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")!;
        }

        [HttpPost("check")]
        public async Task<ResponseModel<bool>> Check([FromBody] AbacCheckRequest request, CancellationToken ct)
        {
            ResponseModel<bool> response = new ResponseModel<bool>();
            if (string.IsNullOrWhiteSpace(request.ActivityKey))
            {
                response.Data = false;
            }
            else
            {

                // Usuario desde el access token
                var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("sub");

                if (string.IsNullOrWhiteSpace(userId))
                {
                    response.Data = false;
                    response.Code = 401;
                }
                else
                {
                    await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_cs);
                    await conn.OpenAsync(ct);

                    await using var cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_IsAllowedActivity", conn)
                    {
                        CommandType = System.Data.CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@AspNetUserId", System.Data.SqlDbType.NVarChar, 450) { Value = userId });
                    cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@ActivityKey", System.Data.SqlDbType.NVarChar, 150) { Value = request.ActivityKey });

                    var scalar = await cmd.ExecuteScalarAsync(ct);
                    response.Data = (scalar != null && Convert.ToBoolean(scalar));
                }

                
            }



            return response;
        }
    }

}
