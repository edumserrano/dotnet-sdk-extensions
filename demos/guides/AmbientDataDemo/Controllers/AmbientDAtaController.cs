using AmbientDataDemo.AmbientData;
using Microsoft.AspNetCore.Mvc;

namespace AmbientDataDemo.Controllers
{
    [ApiController]
    [Route("api/ambient/demo")]
    public class AmbientDAtaController : ControllerBase
    {
        private readonly IMyAmbientDataAccessor _ambientDataAccessor;

        public AmbientDAtaController(IMyAmbientDataAccessor ambientDataAccessor)
        {
            _ambientDataAccessor = ambientDataAccessor;
        }

        [HttpGet]
        public MyAmbientData Get()
        {
            return _ambientDataAccessor.MyAmbientData;
        }
    }
}
