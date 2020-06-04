using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AzureAdAppRolesWebSample.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        // 認証のみ
        [HttpGet]
        public IActionResult Get()
        {
            var roles = User.FindAll(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value).ToArray();

            return Ok(roles);
        }

        // 認可を "User.IsInRole" で。
        [HttpGet("a")]
        public IActionResult GetA()
        {
            if (!User.IsInRole(AppRoles.SettingsReader))
            {
                return Forbid();
            }
            return Ok("a!");
        }

        // アトリビュートで単一 role を定義
        [Authorize(Roles = AppRoles.SettingsReader)]
        [HttpGet("b")]
        public IActionResult GetB() => Ok("b!");

        // アトリビュートで複数 Role を定義: しんどい
        [Authorize(Roles = "Admin, settings-writer")]
        [HttpGet("c")]
        public IActionResult GetC() => Ok("c!");

        // Admin Role 持ってないのでしぬはず
        [Authorize(Roles = AppRoles.Admin)]
        [HttpGet("d")]
        public IActionResult GetD() => Ok("d!");
    }
}