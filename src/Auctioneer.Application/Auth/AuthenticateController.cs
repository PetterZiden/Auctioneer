using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auctioneer.Application.Auth.Models;
using Auctioneer.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auctioneer.Application.Auth;

[AllowAnonymous]
[Route("api/auth")]
public class AuthenticateController : ApiControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticateController> _logger;

    public AuthenticateController(ILogger<AuthenticateController> logger, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager, IConfiguration configuration) : base(logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("token")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetToken([FromBody] LoginUser userToLogin)
    {
        var user = await _userManager.FindByNameAsync(userToLogin.Username);
        if (user is null || !await _userManager.CheckPasswordAsync(user, userToLogin.Password))
            return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

        var token = GetToken(claims);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), expiration = token.ValidTo });
    }

    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Register([FromBody] RegisterUser userToRegister)
    {
        var userExist = await _userManager.FindByNameAsync(userToRegister.Username);
        if (userExist is not null)
            return StatusCode(StatusCodes.Status400BadRequest,
                new AuthResponse { Status = "Error", Message = "User already exists." });

        IdentityUser user = new()
        {
            Email = userToRegister.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = userToRegister.Username
        };

        var result = await _userManager.CreateAsync(user, userToRegister.Password);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new AuthResponse
                    { Status = "Error", Message = "User creation failed! Please check user details and try again." });

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            await _userManager.AddToRoleAsync(user, UserRoles.User);
        }
        else
        {
            await _userManager.AddToRoleAsync(user, UserRoles.User);
        }


        return Ok(new AuthResponse { Status = "Success", Message = "User created successfully." });
    }

    [HttpPost("register-admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUser userToRegister)
    {
        var userExist = await _userManager.FindByNameAsync(userToRegister.Username);
        if (userExist is not null)
            return StatusCode(StatusCodes.Status400BadRequest,
                new AuthResponse { Status = "Error", Message = "User already exists." });

        IdentityUser user = new()
        {
            Email = userToRegister.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = userToRegister.Username
        };

        var result = await _userManager.CreateAsync(user, userToRegister.Password);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new AuthResponse
                    { Status = "Error", Message = "User creation failed! Please check user details and try again." });

        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            await _userManager.AddToRoleAsync(user, UserRoles.Admin);
        }
        else
        {
            await _userManager.AddToRoleAsync(user, UserRoles.Admin);
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            await _userManager.AddToRoleAsync(user, UserRoles.User);
        }
        else
        {
            await _userManager.AddToRoleAsync(user, UserRoles.User);
        }

        return Ok(new AuthResponse { Status = "Success", Message = "User created successfully." });
    }

    private JwtSecurityToken GetToken(IEnumerable<Claim> claims)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: claims,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}