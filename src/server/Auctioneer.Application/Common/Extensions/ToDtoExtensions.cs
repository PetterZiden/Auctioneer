using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.Application.Common.Extensions;

public static class ToDtoExtensions
{
    public static MemberDto ToDto(this Member member) =>
        new()
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Street = member.Address.Street,
            ZipCode = member.Address.Zipcode,
            City = member.Address.City,
            Email = member.Email.Value,
            PhoneNumber = member.PhoneNumber,
            CurrentRating = member.CurrentRating,
            NumberOfRatings = member.NumberOfRatings,
            Bids = member.Bids?.ToList()
        };

    public static List<MemberDto> ToDtos(this IEnumerable<Member> members) =>
        members.Select(member => new MemberDto
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Street = member.Address.Street,
            ZipCode = member.Address.Zipcode,
            City = member.Address.City,
            Email = member.Email.Value,
            PhoneNumber = member.PhoneNumber,
            CurrentRating = member.CurrentRating,
            NumberOfRatings = member.NumberOfRatings,
            Bids = member.Bids?.ToList()
        }).ToList();

    public static AuctionDto ToDto(this Auction auction) =>
        new()
        {
            Id = auction.Id,
            MemberId = auction.MemberId,
            Title = auction.Title,
            Description = auction.Description,
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            StartingPrice = auction.StartingPrice,
            CurrentPrice = auction.CurrentPrice,
            ImgRoute = auction.ImgRoute,
            Bids = auction.Bids?.ToList()
        };

    public static List<AuctionDto> ToDtos(this IEnumerable<Auction> auctions) =>
        auctions.Select(auction => new AuctionDto
        {
            Id = auction.Id,
            MemberId = auction.MemberId,
            Title = auction.Title,
            Description = auction.Description,
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            StartingPrice = auction.StartingPrice,
            CurrentPrice = auction.CurrentPrice,
            ImgRoute = auction.ImgRoute,
            Bids = auction.Bids?.ToList()
        }).ToList();
}