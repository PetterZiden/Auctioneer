using Microsoft.AspNetCore.Identity;

namespace Auctioneer.Application.Auth.Models;

public class AuctioneerUser : IdentityUser
{
    public Guid MemberId { get; set; }
}