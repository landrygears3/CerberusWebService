using CerberusClassLibrary.DataSecure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CerberusWebService.Controllers
{
    [ApiController]
    [Route("api/navigation")]
    public class NavigationConfigController : ControllerBase
    {
        private readonly CerberusDbContext _context;

        public NavigationConfigController(CerberusDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.NavigationConfigs
                .AsNoTracking()
                .OrderBy(x => x.Nivel)
                .ThenBy(x => x.Orden)
                .ToListAsync();

            return Ok(data);
        }
    }
}
