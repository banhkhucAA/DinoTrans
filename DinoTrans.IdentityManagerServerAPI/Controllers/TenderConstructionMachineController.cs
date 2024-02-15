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
    public class TenderConstructionMachineController : ControllerBase
    {
        private readonly ITenderConstructionMachineService _tenderconstructionMachineService;

        public TenderConstructionMachineController(ITenderConstructionMachineService tenderconstructionMachineService)
        {
            _tenderconstructionMachineService = tenderconstructionMachineService;
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> CreateTenderConstructionMachine(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto)
        {
            var result = await _tenderconstructionMachineService.CreateTenderConstructionMachine(dto);
            return Ok(result);
        }
    }
}
