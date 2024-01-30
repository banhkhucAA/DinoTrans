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
    public class ContructionMachineController : ControllerBase
    {
        private readonly IContructionMachineService _contructionMachineService;

        public ContructionMachineController(IContructionMachineService contructionMachineService)
        {
            _contructionMachineService = contructionMachineService;
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> CreateContructionMachine(CreateContructionMachineDTO dto)
        {
            var result = await _contructionMachineService.CreateContructionMachine(dto);
            return Ok(result);
        }
    }
}
