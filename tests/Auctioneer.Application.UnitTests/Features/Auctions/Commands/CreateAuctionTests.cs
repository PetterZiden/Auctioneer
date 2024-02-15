using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Commands;

namespace Auctioneer.Application.UnitTests.Features.Auctions.Commands;

public class CreateAuctionTests
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateAuctionCommandHandler _handler;
    private readonly CreateAuctionCommandValidator _validator = new();

    public CreateAuctionTests()
    {
        _auctionRepository = Substitute.For<IRepository<Auction>>();
        _memberRepository = Substitute.For<IRepository<Member>>();
        _eventRepository = Substitute.For<IRepository<DomainEvent>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateAuctionCommandHandler(_auctionRepository, _memberRepository, _eventRepository,
            _unitOfWork);
    }

    [Fact]
    public async void Should_Return_SuccessResult_And_AuctionId_When_Auction_Is_Created_Successfully()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1).CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Member_Is_Not_Found()
    {
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns((Member?)null);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("No member found");
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0).CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_Title_Is_Null()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var request = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = null,
            Description = "MockediMock",
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = 100,
            ImgRoute = "../images/test.jpg"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("Title can not be empty");
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0).CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_Description_Is_Null()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var request = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "MockAuction",
            Description = null,
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = 100,
            ImgRoute = "../images/test.jpg"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("Description can not be empty");
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_StartTime_Is_Earlier_Than_Current_DateTime()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var request = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "MockAuction",
            Description = "MockediMock",
            StartTime = DateTimeOffset.Now.AddHours(-1),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = 100,
            ImgRoute = "../images/test.jpg"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("Start time can not be earlier than current day and time");
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_EndTime_Is_Earlier_Than_1_Day_In_The_Future()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var request = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "MockAuction",
            Description = "MockediMock",
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddHours(23),
            StartingPrice = 100,
            ImgRoute = "../images/test.jpg"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should()
            .Be("End time can not be earlier than at least one day in the future");
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_StartPrice_Is_Less_Or_Equal_To_0()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var request = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "MockAuction",
            Description = "MockediMock",
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = 0,
            ImgRoute = "../images/test.jpg"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("Starting price must be greater than 0");
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_ImgRoute_Is_Null()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var request = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "MockAuction",
            Description = "MockediMock",
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = 100,
            ImgRoute = null
        };

        var result = await _handler.Handle(request, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("Image route can not be empty");
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("AuctionRepository failed"));
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("AuctionRepository failed");
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1).CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        _memberRepository.GetAsync(Arg.Any<Guid>())!.Returns(
            Task.FromException<Member>(new Exception("MemberRepository failed")));
        _auctionRepository.CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("MemberRepository failed");
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0).CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_EventRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("EventRepository failed"));
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("EventRepository failed");
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(0).CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_UnitOfWork_Throws_Exception()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _auctionRepository.CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(x => throw new Exception("UnitOfWork failed"));


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("UnitOfWork failed");
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _auctionRepository.Received(1).CreateAsync(Arg.Any<Auction>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Null()
    {
        var command = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = null,
            Description = "MockediMock",
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = 100,
            ImgRoute = "../images/test.jpg"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Null()
    {
        var command = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "MockAuction",
            Description = null,
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = 100,
            ImgRoute = "../images/test.jpg"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Description);
    }

    [Fact]
    public void Should_Have_Error_When_StartingPrice_Is_Less_Than_0()
    {
        var command = new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "MockAuction",
            Description = null,
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = -1,
            ImgRoute = "../images/test.jpg"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.StartingPrice);
    }

    private static CreateAuctionCommand GetValidCommand()
    {
        return new CreateAuctionCommand
        {
            MemberId = Guid.NewGuid(),
            Title = "MockAuction",
            Description = "MockediMock",
            StartTime = DateTimeOffset.Now.AddHours(3),
            EndTime = DateTimeOffset.Now.AddDays(7).AddHours(3),
            StartingPrice = 100,
            ImgRoute = "../images/test.jpg"
        };
    }
}