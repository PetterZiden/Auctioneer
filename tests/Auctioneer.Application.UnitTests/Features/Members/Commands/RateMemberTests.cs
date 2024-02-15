using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Commands;

namespace Auctioneer.Application.UnitTests.Features.Members.Commands;

public class RateMemberTests
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RateMemberCommandHandler _handler;
    private readonly RateMemberCommandValidator _validator = new();

    public RateMemberTests()
    {
        _memberRepository = Substitute.For<IRepository<Member>>();
        _eventRepository = Substitute.For<IRepository<DomainEvent>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RateMemberCommandHandler(_memberRepository, _eventRepository, _unitOfWork);
    }

    [Fact]
    public async void Should_Return_SuccessResult_When_Member_Is_Rated_Successfully()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsSuccess.Should().BeTrue();
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
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
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(6)]
    public async void Should_Return_FailedResult_When_Stars_Is_Lower_Than_1_Or_Greater_Than_5(int stars)
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var result = await _handler.Handle(new RateMemberCommand
        {
            RatingForMemberId = Guid.NewGuid(),
            RatingFromMemberId = Guid.NewGuid(),
            Stars = stars
        }, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("Rating must be between 1 and 5");
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("MemberRepository failed"));
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("MemberRepository failed");
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_EventRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
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
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_UnitOfWork_Throws_Exception()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(x => throw new Exception("UnitOfWork failed"));


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("UnitOfWork failed");
        await _memberRepository.Received(2).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public void Should_Have_Error_When_RatingForMemberId_Is_Empty()
    {
        var command = new RateMemberCommand
        {
            RatingForMemberId = Guid.Empty,
            RatingFromMemberId = Guid.NewGuid(),
            Stars = 3
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.RatingForMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_RatingFromMemberId_Is_Empty()
    {
        var command = new RateMemberCommand
        {
            RatingForMemberId = Guid.NewGuid(),
            RatingFromMemberId = Guid.Empty,
            Stars = 3
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.RatingFromMemberId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(6)]
    public void Should_Have_Error_When_Stars_Is_Less_Than_1_And_Greater_Than_5(int stars)
    {
        var command = new RateMemberCommand
        {
            RatingForMemberId = Guid.NewGuid(),
            RatingFromMemberId = Guid.NewGuid(),
            Stars = stars
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Stars);
    }

    private static RateMemberCommand GetValidCommand()
    {
        return new RateMemberCommand
        {
            RatingForMemberId = Guid.NewGuid(),
            RatingFromMemberId = Guid.NewGuid(),
            Stars = 3
        };
    }
}