using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace OrleansChess.Web.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        private readonly IClusterClient _client;

        public HelloController(IClusterClient client)
        {
            _client = client;
        }

        [HttpGet("[action]/{message}")]
        public async Task<string> SayHello(string message)
        {
            var guid = Guid.NewGuid();
            var helloGrain = _client.GetGrain<IHello>(guid);
            var hello = await helloGrain.SayHello(message);
            return hello;
        }
    }
}