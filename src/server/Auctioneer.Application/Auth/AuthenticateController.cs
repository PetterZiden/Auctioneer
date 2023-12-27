using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Auctioneer.Application.Auth.Models;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Validators;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Auth;

[AllowAnonymous]
[Route("api/auth")]
public class AuthenticateController(ILogger<AuthenticateController> logger, IUserService userService)
    : ApiControllerBase(logger)
{
    [HttpPost("token")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetToken([FromBody] LoginUser userToLogin)
    {
        try
        {
            var validationResult = await new LoginUserValidator().ValidateAsync(userToLogin);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var token = await userService.GetAuthToken(userToLogin);

            if (!token.IsSuccess)
                return ReturnError(token.Errors.FirstOrDefault() as Error);

            return Ok(new
                { token = new JwtSecurityTokenHandler().WriteToken(token.Value), expiration = token.Value.ValidTo });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
    }

    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Register([FromBody] RegisterUser userToRegister)
    {
        var validationResult = await new RegisterUserValidator().ValidateAsync(userToRegister);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
            return BadRequest(errorMessages);
        }

        var result = await userService.RegisterUser(userToRegister, false);

        if (!result.IsSuccess)
            return ReturnError(result.Errors.FirstOrDefault() as Error);

        return Ok(new AuthResponse { Status = "Success", Message = "User created successfully." });
    }

    [HttpPost("register-admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUser userToRegister)
    {
        var validationResult = await new RegisterUserValidator().ValidateAsync(userToRegister);
        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
            return BadRequest(errorMessages);
        }

        var result = await userService.RegisterUser(userToRegister, true);

        if (!result.IsSuccess)
            return ReturnError(result.Errors.FirstOrDefault() as Error);

        return Ok(new AuthResponse { Status = "Success", Message = "User created successfully." });
    }
}