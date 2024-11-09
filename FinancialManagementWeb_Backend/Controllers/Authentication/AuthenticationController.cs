using EntityFramework.DbEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.IdentityModel.Tokens;
using ProjectModel.AuthModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace TeamManagementProject_Backend.Controllers.Authentication
{
    [Route("api/authenticate")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthenticationController(IConfiguration config
            , UserManager<CustomUser> userManager
            , RoleManager<IdentityRole> roleManager)
        {
            _config = config;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [Route("login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] ApplicationUser model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(model.UserName);
            }

            var isRightPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (user != null && isRightPassword)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Id),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var tokenString = GenerateJSONWebToken(authClaims);
                TokenInfo loginInfo = new TokenInfo()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(tokenString),
                    UserId = user.Id,
                    Exp = tokenString.ValidTo,
                };

                return Ok(loginInfo);
            }
            else
            {
                throw new("Sai mật khẩu hoặc tên người dùng");
            }
        }

        private JwtSecurityToken GenerateJSONWebToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

    }
}
