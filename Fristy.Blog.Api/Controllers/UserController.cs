﻿using Fristy.Blog.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Fristy.Blog.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        #region Init

        private readonly IUserService _userService;

        public UserController(IUserService commonService)
        {
            this._userService = commonService;
        }

        #endregion

        #region Users

        [HttpPost("GetToken")]
        public async Task<IActionResult> GetAccessToken([FromBody]LoginViewModel userModel)
        {
            try
            {
                var access = await _userService.GetAccessToken(userModel);
                return Ok(access);
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser([FromBody]UserViewModel userModel)
        {
            if (string.IsNullOrEmpty(userModel.UserEmail) ||
                string.IsNullOrEmpty(userModel.Password))
            {
                return StatusCode(500, "UserEmail/Password id required");
            }

            try
            {
                if (await _userService.RegisterUser(userModel))
                    return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return StatusCode(500);
        }

        [HttpGet("isUniqueMail")]
        public async Task<bool> IsUniqueEmail(string eMail, string userId)
        {
            return await _userService.IsUniqueEmail(eMail, userId);
        }

        [HttpGet("DeleteUser")]
        public async Task<bool> DeleteUser(string eMail)
        {
            return await _userService.DeleteUser(eMail);
        }

        #endregion
    }
}