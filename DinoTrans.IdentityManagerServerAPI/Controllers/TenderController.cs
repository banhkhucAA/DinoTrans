﻿using DinoTrans.IdentityManagerServerAPI.Services.Implements;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.DTOs.UserResponse;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Implements;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class TenderController : ControllerBase
    {
        private readonly ITenderService _tenderService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationUser _currentUser;
        private readonly IUserService _userService;

        public TenderController(ITenderService tenderService, IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _tenderService = tenderService;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;

            var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null && identity.Claims.Any())
            {
                var userIdParse = int.TryParse(identity.FindFirst(ClaimTypes.NameIdentifier).Value, out int userId);
                if (userIdParse)
                {
                    var user = _userService.GetUserById(userId);
                    if(user!=null)
                    _currentUser = user.Data;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenderStep1(CreateTenderStep1DTO dto)
        {
            var result = await _tenderService.CreateTenderStep1(dto);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenderStep2(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto)
        {
            var result = await _tenderService.CreateTenderStep2(dto);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> StartTender([FromBody]int TenderId)
        {
            var result = await _tenderService.StartTender(TenderId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SearchActiveBy([FromBody] SearchTenderActiveDTO dto)
        {
            var result = await _tenderService.SearchActiveBy(dto, _currentUser);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatusAuto([FromBody] List<int> TenderIds)
        {
            var result = await _tenderService.UpdateStatusAuto(TenderIds);
            return Ok(result);
        }
    }
}
