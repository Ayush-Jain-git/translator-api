using Microsoft.AspNetCore.Mvc;
using TranslatorAPI.Models;
using TranslatorAPI.Services;

namespace TranslatorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslateController : ControllerBase
    {
        private readonly TranslationService _service;

        public TranslateController(TranslationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
        {
            var translated = await _service.TranslateAsync(
                request.Text,
                request.TargetLanguage
            );

            return Ok(new { translation = translated });
        }
    }
}