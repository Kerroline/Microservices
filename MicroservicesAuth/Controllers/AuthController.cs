using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MSAuth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSAuth.Services;
using Microsoft.AspNetCore.Authorization;

namespace MSAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        private readonly IJWTService _jwtService;
        private readonly IUserService _userService;

        public AuthController(
            IAuthService authService, 
            IJWTService jwtService,
            IUserService userService)
        {
            this._authService = authService;
            this._jwtService = jwtService;
            this._userService = userService;
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest model)
        {
            var verifiedUser = await _authService.VerifyingUserAsync(model.Username, model.Password);

            if(verifiedUser is not null)
            {
                var pairTokens = await _jwtService.GeneratePairTokensAsync(verifiedUser);
                
                SetTokenCookie(pairTokens.RefreshToken);

                return Ok(pairTokens);
            } 
            else
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var newRefreshToken = await _jwtService.RefreshTokenAsync(refreshToken);

                if(newRefreshToken is not null)
                {
                    var currentUser = await _userService.GetByIdAsync(newRefreshToken.ByUserId);
                    if (currentUser is not null)
                    {
                        var jwsToken = _jwtService.GenerateJWSToken(currentUser);

                        SetTokenCookie(newRefreshToken.Token);

                        return Ok(jwsToken);
                    }
                    else
                    {
                        return StatusCode(500, "user not found");
                    }
                }
                return Unauthorized(new { message = "Invalid token" });  
            }
            return BadRequest(new { message = "refreshToken not found" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(token)) 
            {
                bool response = await _jwtService.RevokeToken(token);

                if (!response)
                    return NotFound(new { message = "Token not found" });

                DeleteTokenCookie();
                return Ok(new { message = "Token revoked" });
            }
            return BadRequest(new { message = "Token is required" }); 
        }


        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(30)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
        private void DeleteTokenCookie()
        {
            Response.Cookies.Delete("refreshToken");
        }
    }
}
