using Microsoft.AspNetCore.Mvc;
using TranslatorAPI.Models;
using TranslatorAPI.Services;

namespace TranslatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/translate")]
    public class TranslateController : ControllerBase
    {
        private readonly TranslationService _service;

        public TranslateController(TranslationService service)
        {
            _service = service;
        }
        [HttpGet]
    public IActionResult Get()
    {
        return Ok("GET working");
    }

        [HttpPost]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
        {
             return Ok(new { translation = "POST working" });
            /*var translated = await _service.TranslateAsync(
                request.Text,
                request.TargetLanguage
            );

            return Ok(new { translation = translated });*/
        }
    }
}