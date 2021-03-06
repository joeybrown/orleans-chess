using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace OrleansChess.Web.Controllers {
    [Route ("api/[controller]/[action]")]
    [ApiController]
    public class UserController : Controller {
        private readonly IClusterClient _client;

        public UserController (IClusterClient client) {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> EnsureAuthenticated () {
            var userIsAuthenticated = User.Claims.Select (x => x.Type).Contains ("userId");

            if (userIsAuthenticated)
                return Ok ();

            var userId = Guid.NewGuid ().ToString ();
            var claims = new Claim[] {
                new Claim ("userId", userId)
            };
            var claimsIdentity = new ClaimsIdentity (claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties ();
            await HttpContext.SignInAsync (
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal (claimsIdentity),
                authProperties);
            return Ok ();
        }

        [HttpGet ("{userName}")]
        public async Task<IActionResult> Login (string userName) {
            var userId = Guid.NewGuid ().ToString ();
            var claims = new Claim[] {
                new Claim ("userId", userId),
                new Claim ("userName", userName),
            };
            var claimsIdentity = new ClaimsIdentity (claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties () {
                IsPersistent = true
            };
            await HttpContext.SignInAsync (
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal (claimsIdentity),
                authProperties);
            return Ok ();
        }

        [HttpGet ("[action]")]
        [Authorize]
        public async Task<IActionResult> Test () {
            var claims = User.Claims;
            return Ok ();
        }

        [HttpGet ("[action]")]
        [Authorize]
        public async Task<IActionResult> IsAuthorized () {
            return Ok (true);
        }
    }
}