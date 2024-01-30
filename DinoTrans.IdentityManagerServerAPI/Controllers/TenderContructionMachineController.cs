using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TenderContructionMachineController : ControllerBase
    {
        private readonly ITenderContructionMachineService _tendercontructionMachineService;

        public TenderContructionMachineController(ITenderContructionMachineService tendercontructionMachineService)
        {
            _tendercontructionMachineService = tendercontructionMachineService;
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> CreateTenderContructionMachine(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto)
        {
            var result = await _tendercontructionMachineService.CreateTenderContructionMachine(dto);
            return Ok(result);
        }
    }
}
