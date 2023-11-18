using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auctioneer.Application.Auth.Models;
using Auctioneer.Application.Common.Errors;
using Auctioneer.Application.Common.Interfaces;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Auctioneer.Application.Auth.Services;

public class UserService : IUserService
{
    private readonly UserManager<AuctioneerUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<AuctioneerUser> userManager, RoleManager<IdentityRole> roleManager,
        IConfiguration configuration, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<JwtSecurityToken>> GetAuthToken(LoginUser userToLogin)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(userToLogin.Username);
            if (user is null || !await _userManager.CheckPasswordAsync(user, userToLogin.Password))
                return Result.Fail(new NotAuthorizedError());

            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserName!),
                new("MemberId", user.MemberId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

            var token = GetToken(claims);

            return Result.Ok(token);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }

    public async Task<Result> RegisterUser(RegisterUser userToRegister, bool isAdmin)
    {
        try
        {
            var userExist = await _userManager.FindByNameAsync(userToRegister.Username);
            if (userExist is not null)
                return Result.Fail(new BadRequestError("User already exists."));

            AuctioneerUser user = new()
            {
                MemberId = Guid.NewGuid(),
                Email = userToRegister.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = userToRegister.Username
            };

            var result = await _userManager.CreateAsync(user, userToRegister.Password);
            if (!result.Succeeded)
                return Result.Fail(new Error("User creation failed! Please check user details and try again."));

            if (isAdmin)
            {
                if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                    await _userManager.AddToRoleAsync(user, UserRoles.Admin);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Admin);
                }
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

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
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