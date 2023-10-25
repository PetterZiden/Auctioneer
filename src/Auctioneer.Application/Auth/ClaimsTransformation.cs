using System.Security.Claims;
using Auctioneer.Application.Auth.Models;
using Auctioneer.Application.Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Auctioneer.Application.Auth;

public class ClaimsTransformation : IClaimsTransformation
{
    private readonly CurrentUserService _currentUser;
    private readonly UserManager<AuctioneerUser> _userManager;

    public ClaimsTransformation(CurrentUserService currentUser, UserManager<AuctioneerUser> userManager)
    {
        _currentUser = currentUser;
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        _currentUser.Principal = principal;

        if (principal.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } name)
        {
            _currentUser.User = await _userManager.FindByNameAsync(name);
        }

        return principal;
    }
}