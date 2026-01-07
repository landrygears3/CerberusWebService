using Microsoft.AspNetCore.Mvc;

namespace CerberusWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ABACController : ControllerBase
    {
        [HttpPost("reglas")]
        public async Task<string> Reglas()
        {
            return "reglasAbac";
        }
        
    }
}
