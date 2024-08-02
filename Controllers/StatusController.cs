using Microsoft.AspNetCore.Mvc;
using SMS.Enums;
using SMS.Interfaces;

namespace SMS.Controllers
{
    [Route("api/status")]
    [ApiController]
    public class StatusController : Controller
    {
        public StatusController()
        {
           
        }

        [HttpGet]
        [Route("")]
        public string GetStatus()
        {
            try
            {

                return "Api is running!";

            }
            catch (Exception ex)
            {
                return $"{ex}";
            }
        }
    }
}
