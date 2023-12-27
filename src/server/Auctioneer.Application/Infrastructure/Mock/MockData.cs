using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.Application.Infrastructure.Mock;

public static class MockData
{
    public static List<MemberDto> GetMemberDtoData()
    {
        return
        [
            new MemberDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Mock",
                LastName = "Mocksson",
                Street = "Mockvägen 2",
                ZipCode = "22222",
                City = "Mockholm",
                Email = "Mock@mocky.se",
                PhoneNumber = "0732223344",
                CurrentRating = 0,
                NumberOfRatings = 0,
                Bids = []
            },

            new MemberDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Testsson",
                Street = "Testgränd 2",
                ZipCode = "33333",
                City = "Testborg",
                Email = "Test@testy.se",
                PhoneNumber = "0733332244",
                CurrentRating = 0,
                NumberOfRatings = 0,
                Bids = []
            }
        ];
    }

    public static List<AuctionDto> GetAuctionDtoData()
    {
        return
        [
            new AuctionDto
            {
                Id = Guid.NewGuid(),
                MemberId = Guid.NewGuid(),
                Title = "Mock",
                Description = "MockAuction",
                StartTime = DateTimeOffset.Now,
                EndTime = DateTimeOffset.Now.AddDays(7),
                StartingPrice = 100,
                CurrentPrice = 0,
                ImgRoute = null,
                Bids = []
            },

            new AuctionDto
            {
                Id = Guid.NewGuid(),
                MemberId = Guid.NewGuid(),
                Title = "Test",
                Description = "TestAuction",
                StartTime = DateTimeOffset.Now.AddDays(1),
                EndTime = DateTimeOffset.Now.AddDays(8),
                StartingPrice = 300,
                CurrentPrice = 0,
                ImgRoute = null,
                Bids = []
            }
        ];
    }
}