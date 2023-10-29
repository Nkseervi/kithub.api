using kithub.api.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace kithub.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<KithubUser> _userManager;
        private readonly IConfiguration _config;

        public TokenController(UserManager<KithubUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string username, string password, string grant_type)
        {
            if (await IsValidUsernameAndPassword(username, password))
            {
                return new ObjectResult(await GenerateToken(username));
            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<bool> IsValidUsernameAndPassword(string username, string password)
        {

            var user = await _userManager.FindByEmailAsync(username);
            return await _userManager.CheckPasswordAsync(user, password);
        }

        private async Task<dynamic> GenerateToken(string username)
        {
            var user = await _userManager.FindByEmailAsync(username);
            var roles = await _userManager.GetRolesAsync(user);                

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username ),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            string key = _config.GetValue<string>("Secrets:SecurityKey");

            var token = new JwtSecurityToken(
                            new JwtHeader(
                                new SigningCredentials(
                                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                                SecurityAlgorithms.HmacSha256)),
                            new JwtPayload(claims));

            var output = new
            {
                Access_Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserName = username
            };

            return output;
        }
    }
}
