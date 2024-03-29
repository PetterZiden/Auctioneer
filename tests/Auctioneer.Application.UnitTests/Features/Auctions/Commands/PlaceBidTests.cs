using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Commands;

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
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsSuccess.Should().BeTrue();
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Auction_Is_Not_Found()
    {
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns((Auction?)null);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("No auction found");
        await _memberRepository.Received(0).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Member_Is_Not_Found()
    {
        var validAuction = GetValid.Auction();
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns((Member?)null);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("No member found");
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_CurrentPrice_Is_Greater_Than_Bid()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var request = new PlaceBidCommand
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 50
        };

        var result = await _handler.Handle(request, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().StartWith("Bid must be greater than current price: ");
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_CurrentPrice_Is_Less_Or_Equal_To_0()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var request = new PlaceBidCommand
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 0
        };

        var result = await _handler.Handle(request, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("Bid must be greater than 0");
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("AuctionRepository failed"));
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("AuctionRepository failed");
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>())!.Returns(
            Task.FromException<Member>(new Exception("MemberRepository failed")));
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("MemberRepository failed");
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_EventRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("EventRepository failed"));
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("EventRepository failed");
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_UnitOfWork_Throws_Exception()
    {
        var validMember = GetValid.Member();
        var validAuction = GetValid.Auction();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(x => throw new Exception("UnitOfWork failed"));


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("UnitOfWork failed");
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    private static PlaceBidCommand GetValidCommand()
    {
        return new PlaceBidCommand
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 110
        };
    }
}