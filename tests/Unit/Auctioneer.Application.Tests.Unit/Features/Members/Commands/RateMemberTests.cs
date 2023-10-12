using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Commands;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Auctioneer.Application.Tests.Unit.Features.Members.Commands;

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
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Member_Is_Not_Found()
    {
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns((Member?)null);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No member found", result.Errors[0].Message);
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
            Rating = new Rating
            {
                RatingForMemberId = Guid.NewGuid(),
                RatingFromMemberId = Guid.NewGuid(),
                Stars = stars
            }
        }, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Rating must be between 1 and 5", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>())
            .Returns(x => throw new Exception("MemberRepository failed"));
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("MemberRepository failed", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_EventRepository_Throws_Exception()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>())
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
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(x => throw new Exception("UnitOfWork failed"));


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("UnitOfWork failed", result.Errors[0].Message);
    }

    [Fact]
    public void Should_Have_Error_When_RatingForMemberId_Is_Empty()
    {
        var command = new RateMemberCommand
        {
            Rating = new Rating
            {
                RatingForMemberId = Guid.Empty,
                RatingFromMemberId = Guid.NewGuid(),
                Stars = 3
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Rating.RatingForMemberId);
    }

    [Fact]
    public void Should_Have_Error_When_RatingFromMemberId_Is_Empty()
    {
        var command = new RateMemberCommand
        {
            Rating = new Rating
            {
                RatingForMemberId = Guid.NewGuid(),
                RatingFromMemberId = Guid.Empty,
                Stars = 3
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Rating.RatingFromMemberId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(6)]
    public void Should_Have_Error_When_Stars_Is_Less_Than_1_And_Greater_Than_5(int stars)
    {
        var command = new RateMemberCommand
        {
            Rating = new Rating
            {
                RatingForMemberId = Guid.NewGuid(),
                RatingFromMemberId = Guid.NewGuid(),
                Stars = stars
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Rating.Stars);
    }

    private static RateMemberCommand GetValidCommand()
    {
        return new RateMemberCommand
        {
            Rating = new Rating
            {
                RatingForMemberId = Guid.NewGuid(),
                RatingFromMemberId = Guid.NewGuid(),
                Stars = 3
            }
        };
    }
}