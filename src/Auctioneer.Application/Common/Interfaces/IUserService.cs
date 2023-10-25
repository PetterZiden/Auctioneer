using System.IdentityModel.Tokens.Jwt;
using Auctioneer.Application.Auth.Models;
using FluentResults;

namespace Auctioneer.Application.Common.Interfaces;

public interface IUserService
{
    public Task<Result<JwtSecurityToken>> GetAuthToken(LoginUser userToLogin);
    public Task<Result> RegisterUser(RegisterUser userToRegister, bool isAdmin);
}