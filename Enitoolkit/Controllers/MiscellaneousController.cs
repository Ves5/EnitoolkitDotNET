using Enitoolkit.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Reflection;

namespace Enitoolkit.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class MiscellaneousController : ControllerBase
    {
        private readonly ILogger<MiscellaneousController> _logger;


        public MiscellaneousController(ILogger<MiscellaneousController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ActionName("version")]
        public IResult GetVersion()
        {
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            return Results.Ok(version.Contains("+") ? version.Split("+")[0] : version) ;
        }

        [HttpGet]
        [ActionName("links")]
        public IResult GetLinks()
        {
            var text = System.IO.File.ReadAllText(Environment.CurrentDirectory + "/Misc/links.json");
            var json = JsonSerializer.Deserialize<List<MiscLink>>(text);
            return Results.Ok(json);
        }


        [HttpPost]
        [ActionName("link")]
        public IResult PostLink(string key, string title, string url)
        {
            return Results.Ok();
        }
    }
}
