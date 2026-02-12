using CerberusClassLibrary.DataSecure;
using CerberusClassLibrary.Model;
using CerberusClassLibrary.Model.Navigation;
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
        public async Task<ResponseModel<List<NavigationConfig>>> GetAll()
        {
            ResponseModel<List<NavigationConfig>> response = new ResponseModel<List<NavigationConfig>>();
            try
            {
            List<NavigationConfig> data = await _context.NavigationConfigs
                .AsNoTracking()
                .OrderBy(x => x.Nivel)
                .ThenBy(x => x.Orden)
                .ToListAsync();
                response.Data = data;
                response.IsSuccess = true;
                response.Code = 200;
                response.Message = "Configuraciones de navegación recibidas correctamente.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Code = 500;
                response.Message = "Error recibiendo las configuraciones de navegación.";
                response.Desc = ex.Message;
                response.Data = null;
                return response;
            }


            return response;
        }
    }
}
