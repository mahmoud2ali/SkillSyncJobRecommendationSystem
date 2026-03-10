using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configration;

        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration) {
            this.userManager = userManager;
            _configration = configuration;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUserDto UserFromRequest)
        {
            if(ModelState.IsValid) 
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = UserFromRequest.UserName;
                user.Email = UserFromRequest.Email;

                IdentityResult result = 
                    await userManager.CreateAsync(user, UserFromRequest.Password);
                if (result.Succeeded)
                {
                    return Ok("Created");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto userFromRequest)
        {
            if(ModelState.IsValid)
            {
                ApplicationUser userFromDb = await userManager.FindByNameAsync(userFromRequest.UserName);

                if(userFromDb != null)
                {
                    bool isPasswordValid = await userManager.CheckPasswordAsync(userFromDb, userFromRequest.Password);
                    if(isPasswordValid)
                    {
                        List<Claim> userClaims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.NameIdentifier, userFromDb.Id),
                            new Claim(ClaimTypes.Name, userFromDb.UserName),

                        }; 

                        var userRoles = await userManager.GetRolesAsync(userFromDb);
                        foreach (var userRole in userRoles)
                        {
                            userClaims.Add(new Claim(ClaimTypes.Role, userRole));
                        }


                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configration["JWT:SecretKey"]));


                        SigningCredentials credentialKey = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        JwtSecurityToken token = new JwtSecurityToken(
                            //issuer: _configration["Issuer"],
                            claims: userClaims,
                            expires: DateTime.Now.AddHours(1),
                            signingCredentials: credentialKey
                        );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }  

                ModelState.AddModelError("InvalidCredentials", "Invalid username or password");

            }

            return BadRequest(ModelState);
        }
    }
}
