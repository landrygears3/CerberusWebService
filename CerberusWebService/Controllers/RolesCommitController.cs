using CerberusClassLibrary.Controller.Abac;
using CerberusClassLibrary.Model;
using CerberusClassLibrary.Model.Abac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CerberusWebService.Controllers
{
        [ApiController]
    [Route("api/[controller]")]
    public class RolesCommitController : ControllerBase
    {
        private readonly AbacCommitService _abacCommitService;

        public RolesCommitController(AbacCommitService abacCommitService)
        {

            _abacCommitService = abacCommitService;
        }

        [Authorize]
        [HttpPost("commit")]
        public async Task<ResponseModel<bool>> CommitGeneral([FromBody] SavePersonaAccessRequest request, CancellationToken ct)
        {
            // Usuario que hace el commit -> desde token
            var aspNetUserIdFromToken =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            var resp = await _abacCommitService.CommitAccesosAsync(aspNetUserIdFromToken!, request, ct);

            // Siempre Ok() con ResponseModel<T> como acordaste
            return resp;
        }

        [Authorize]
        [HttpPost("ObtenPersonaAccess")]
        public async Task<ResponseModel<PersonaAccessResponse>> ObtenPersonaAccess(
             [FromBody] GetPersonaAccessRequest request,
             CancellationToken ct)
        {
            var aspNetUserIdFromToken =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(aspNetUserIdFromToken))
            {
                return new ResponseModel<PersonaAccessResponse>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "No autorizado",
                    Desc = "No se pudo obtener el usuario del token.",
                    Data = null!
                };
            }

            var resp = await _abacCommitService.GetPersonaAccessAsync(
                aspNetUserIdFromToken,
                request?.NumeroUsuario ?? string.Empty,
                ct);

            return resp;
        }
    }
}
