using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register (UserDTO userDTO)
        {
            var response = await _userService.CreateAccount(userDTO);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Login (LoginDTO loginDTO)
        {
            var response = await _userService.LoginAccount(loginDTO);
            return Ok(response);
        }

    }
}
