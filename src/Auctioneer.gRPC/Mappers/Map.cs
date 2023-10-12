using Google.Protobuf.WellKnownTypes;

namespace Auctioneer.gRPC.Mappers;

public static class Map
{
    public static MemberModel ApplicationMemberToMemberModel(Auctioneer.Application.Entities.Member member) =>
        new()
        {
            Id = member.Id.ToString(),
            FirstName = member.FirstName,
            LastName = member.LastName,
            Street = member.Address.Street,
            Zipcode = member.Address.ZipCode,
            City = member.Address.City,
            Email = member.Email,
            PhoneNumber = member.PhoneNumber,
            CurrentRating = member.CurrentRating,
            NumberOfRatings = member.NumberOfRatings
        };


    public static List<MemberModel> ApplicationMemberToMemberModelList(
        IEnumerable<Application.Entities.Member> members) =>
        members.Select(m => new MemberModel
        {
            Id = m.Id.ToString(),
            FirstName = m.FirstName,
            LastName = m.LastName,
            Street = m.Address.Street,
            Zipcode = m.Address.ZipCode,
            City = m.Address.City,
            Email = m.Email,
            PhoneNumber = m.PhoneNumber,
            CurrentRating = m.CurrentRating,
            NumberOfRatings = m.NumberOfRatings
        }).ToList();

    public static AuctionModel ApplicationAuctionToAuctionModel(Auctioneer.Application.Entities.Auction auction) =>
        new()
        {
            Id = auction.Id.ToString(),
            MemberId = auction.MemberId.ToString(),
            Title = auction.Title,
            Description = auction.Description,
            StartTime = Timestamp.FromDateTimeOffset(auction.StartTime),
            EndTime = Timestamp.FromDateTimeOffset(auction.EndTime),
            StartingPrice = (double)auction.StartingPrice,
            CurrentPrice = (double)auction.CurrentPrice,
            ImgRoute = auction.ImgRoute
        };

    public static List<AuctionModel> ApplicationAuctionToAuctionModelList(
        IEnumerable<Auctioneer.Application.Entities.Auction> auctions) =>
        auctions.Select(a => new AuctionModel
        {
            Id = a.Id.ToString(),
            MemberId = a.MemberId.ToString(),
            Title = a.Title,
            Description = a.Description,
            StartTime = Timestamp.FromDateTimeOffset(a.StartTime),
            EndTime = Timestamp.FromDateTimeOffset(a.EndTime),
            StartingPrice = (double)a.StartingPrice,
            CurrentPrice = (double)a.CurrentPrice,
            ImgRoute = a.ImgRoute
        }).ToList();
}