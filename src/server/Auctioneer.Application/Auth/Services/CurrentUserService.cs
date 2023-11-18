using System.Security.Claims;
using Auctioneer.Application.Auth.Models;

namespace Auctioneer.Application.Auth.Services;

public class CurrentUserService
{
    public AuctioneerUser User { get; set; }
    public ClaimsPrincipal Principal { get; set; } = default!;

    public string UserName => Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public Guid MemberId => Guid.Parse(Principal.FindFirstValue("MemberId") ?? string.Empty);
    public bool IsAdmin => Principal.IsInRole("admin");
}