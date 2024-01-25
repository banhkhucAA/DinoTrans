using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TenderController : ControllerBase
    {
        private readonly ITenderService _tenderService;
        public TenderController(ITenderService tenderService)
        {
            _tenderService = tenderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenderStep1(CreateTenderStep1DTO dto)
        {
            var result = await _tenderService.CreateTenderStep1(dto);
            return Ok(result);
        }
    }
}
