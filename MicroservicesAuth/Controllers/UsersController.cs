using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSAuth.Models;
using MSAuth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            this._userService = userService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userList = await _userService.GetAllAsync();
            return Ok(userList);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateCustomUserRequest model)
        {
            var candidate =  await _userService.FindUserAsync(model.Username, model.Email);
            if(candidate is null)
            {
                var newUser = await _userService.CreateAsync(model);

                return Created(newUser.Id, newUser);
            }
            return BadRequest(new { message = "Username or Email is exist" });
        }


    }
}
