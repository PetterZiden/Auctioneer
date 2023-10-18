using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Commands;
using NSubstitute;

namespace Auctioneer.Application.UnitTests.Features.Auctions.Commands;

public class PlaceBidTests
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PlaceBidCommandHandler _handler;

    public PlaceBidTests()
    {
        _auctionRepository = Substitute.For<IRepository<Auction>>();
        _memberRepository = Substitute.For<IRepository<Member>>();
        _eventRepository = Substitute.For<IRepository<DomainEvent>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new PlaceBidCommandHandler(_auctionRepository, _memberRepository, _eventRepository, _unitOfWork);
    }

    [Fact]
    public async void Should_Return_SuccessResult_When_Bid_Is_Placed_Successfully()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Auction_Is_Not_Found()
    {
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns((Auction?)null);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No auction found", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Member_Is_Not_Found()
    {
        var validAuction = GetValid.Auction();
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns((Member?)null);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No member found", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_CurrentPrice_Is_Greater_Than_Bid()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var request = new PlaceBidCommand
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 50,
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.StartsWith("Bid must be greater than current price: ", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_CurrentPrice_Is_Less_Or_Equal_To_0()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var request = new PlaceBidCommand
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 0,
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Bid must be greater than 0", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("AuctionRepository failed"));
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("AuctionRepository failed", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>())!.Returns(
            Task.FromException<Member>(new Exception("MemberRepository failed")));
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("MemberRepository failed", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_EventRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("EventRepository failed"));
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("EventRepository failed", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_UnitOfWork_Throws_Exception()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(x => throw new Exception("UnitOfWork failed"));


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("UnitOfWork failed", result.Errors[0].Message);
    }

    private static PlaceBidCommand GetValidCommand()
    {
        return new PlaceBidCommand
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 110,
        };
    }
}