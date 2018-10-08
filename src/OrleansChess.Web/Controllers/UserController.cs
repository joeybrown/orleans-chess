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
        public async Task<IActionResult> EnsureUserHasPlayerId () {
            var userIsAuthenticated = User.Claims.Select (x => x.Type).Contains ("playerId");

            if (userIsAuthenticated)
                return Ok ();

            var playerId = Guid.NewGuid ().ToString ();
            var claims = new Claim[] {
                new Claim ("playerId", playerId)
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
    }
}