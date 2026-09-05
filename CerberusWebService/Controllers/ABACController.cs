using CerberusClassLibrary.Controller.Abac;
using CerberusClassLibrary.Interfaz;
using CerberusClassLibrary.Model;
using CerberusClassLibrary.Model.Abac;
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

        [HttpPost("resolver-destinatarios")]
        public async Task<IActionResult>
    ResolverDestinatarios(
        [FromBody]
        ResolverDestinatariosAbacRequest request,

        [FromServices]
        AbacDestinatariosFunctions destinatariosFunctions,

        CancellationToken cancellationToken)
        {
            try
            {
                //----------------------------------------------------------
                // Validación
                //----------------------------------------------------------

                if (request == null)
                {
                    return BadRequest(
                        new ResponseModel<
                            List<ResolverDestinatariosAbacResponse>>
                        {
                            IsSuccess = false,
                            Code = 400,
                            Message = "Solicitud inválida.",
                            Desc =
                                "El cuerpo de la solicitud es obligatorio.",
                            Data =
                                new List<
                                    ResolverDestinatariosAbacResponse>()
                        });
                }


                request.ActividadIds ??= new List<int>();
                request.DepartamentoIds ??= new List<int>();
                request.RolIds ??= new List<int>();


                //----------------------------------------------------------
                // Quitar IDs inválidos y duplicados
                //----------------------------------------------------------

                request.ActividadIds =
                    request.ActividadIds
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();


                request.DepartamentoIds =
                    request.DepartamentoIds
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();


                request.RolIds =
                    request.RolIds
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();


                //----------------------------------------------------------
                // Debe venir al menos un criterio
                //----------------------------------------------------------

                var tieneCriterios =
                    request.ActividadIds.Count > 0 ||
                    request.DepartamentoIds.Count > 0 ||
                    request.RolIds.Count > 0;


                if (!tieneCriterios)
                {
                    return BadRequest(
                        new ResponseModel<
                            List<ResolverDestinatariosAbacResponse>>
                        {
                            IsSuccess = false,
                            Code = 400,
                            Message = "Criterios inválidos.",
                            Desc =
                                "Debe especificarse al menos una actividad, departamento o rol.",
                            Data =
                                new List<
                                    ResolverDestinatariosAbacResponse>()
                        });
                }


                //----------------------------------------------------------
                // MatchMode
                //----------------------------------------------------------

                request.MatchMode =
                    string.IsNullOrWhiteSpace(
                        request.MatchMode)
                    ? "ANY"
                    : request.MatchMode
                        .Trim()
                        .ToUpperInvariant();


                if (request.MatchMode != "ANY" &&
                    request.MatchMode != "ALL")
                {
                    return BadRequest(
                        new ResponseModel<
                            List<ResolverDestinatariosAbacResponse>>
                        {
                            IsSuccess = false,
                            Code = 400,
                            Message = "MatchMode inválido.",
                            Desc =
                                "MatchMode solamente puede ser ANY o ALL.",
                            Data =
                                new List<
                                    ResolverDestinatariosAbacResponse>()
                        });
                }


                //----------------------------------------------------------
                // Resolver ABAC
                //----------------------------------------------------------

                var resultado =
                    await destinatariosFunctions
                        .ResolverDestinatariosAsync(
                            request,
                            cancellationToken);


                //----------------------------------------------------------
                // Response estándar Cerberus
                //----------------------------------------------------------

                return Ok(
                    new ResponseModel<
                        List<ResolverDestinatariosAbacResponse>>
                    {
                        IsSuccess = true,
                        Code = 200,
                        Message =
                            "Destinatarios obtenidos correctamente.",
                        Desc =
                            $"Se encontraron {resultado.Count} destinatarios.",
                        Data = resultado
                    });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(
                    StatusCodes.Status408RequestTimeout,

                    new ResponseModel<
                        List<ResolverDestinatariosAbacResponse>>
                    {
                        IsSuccess = false,
                        Code = 408,
                        Message = "Solicitud cancelada.",
                        Desc =
                            "La operación fue cancelada antes de finalizar.",
                        Data =
                            new List<
                                ResolverDestinatariosAbacResponse>()
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,

                    new ResponseModel<
                        List<ResolverDestinatariosAbacResponse>>
                    {
                        IsSuccess = false,
                        Code = 500,
                        Message =
                            "Error al resolver destinatarios ABAC.",
                        Desc = ex.Message,
                        Data =
                            new List<
                                ResolverDestinatariosAbacResponse>()
                    });
            }
        }
    }

}
