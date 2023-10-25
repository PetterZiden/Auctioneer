using Auctioneer.Application.Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auctioneer.Application.Auth;

public class ApplicationDbContext : IdentityDbContext<AuctioneerUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AuctioneerUser>()
            .Property(i => i.MemberId)
            .IsRequired();
    }
}