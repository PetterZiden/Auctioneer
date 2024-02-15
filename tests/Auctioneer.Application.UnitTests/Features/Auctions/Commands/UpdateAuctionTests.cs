using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Commands;

namespace Auctioneer.Application.UnitTests.Features.Auctions.Commands;

public class UpdateAuctionTests
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateAuctionCommandHandler _handler;
    private readonly UpdateAuctionCommandValidator _validator = new();

    public UpdateAuctionTests()
    {
        _auctionRepository = Substitute.For<IRepository<Auction>>();
        _eventRepository = Substitute.For<IRepository<DomainEvent>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateAuctionCommandHandler(_auctionRepository, _eventRepository, _unitOfWork);
    }

    [Fact]
    public async void Should_Return_SuccessResult_When_Auction_Is_Deleted_Successfully()
    {
        var validAuction = GetValid.Auction();

        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsSuccess.Should().BeTrue();
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
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        var validAuction = GetValid.Auction();
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("AuctionRepository failed"));
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("AuctionRepository failed");
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_EventRepository_Throws_Exception()
    {
        var validAuction = GetValid.Auction();
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("EventRepository failed"));
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("EventRepository failed");
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_UnitOfWork_Throws_Exception()
    {
        var validAuction = GetValid.Auction();
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);
        _auctionRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(x => throw new Exception("UnitOfWork failed"));


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("UnitOfWork failed");
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Null()
    {
        var command = new UpdateAuctionCommand
        {
            Id = Guid.NewGuid(),
            Title = null,
            Description = "MockediMock",
            ImgRoute = "../images/test.jpg"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Null()
    {
        var command = new UpdateAuctionCommand
        {
            Id = Guid.NewGuid(),
            Title = "TestAuction",
            Description = null,
            ImgRoute = "../images/test.jpg"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new UpdateAuctionCommand
        {
            Id = Guid.Empty,
            Title = "TestAuction",
            Description = "MockediMock",
            ImgRoute = "../images/test.jpg"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Id);
    }

    private static UpdateAuctionCommand GetValidCommand()
    {
        return new UpdateAuctionCommand
        {
            Id = Guid.NewGuid(),
            Title = "MockAuction",
            Description = "MockediMock",
            ImgRoute = "../images/test.jpg"
        };
    }
}