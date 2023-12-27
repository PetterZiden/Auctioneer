using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Commands;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Auctioneer.Application.UnitTests.Features.Members.Commands;

public class ChangeEmailMemberTests
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ChangeEmailMemberCommandHandler _handler;
    private readonly ChangeEmailMemberCommandValidator _validator = new();

    public ChangeEmailMemberTests()
    {
        _memberRepository = Substitute.For<IRepository<Member>>();
        _eventRepository = Substitute.For<IRepository<DomainEvent>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ChangeEmailMemberCommandHandler(_memberRepository, _eventRepository, _unitOfWork);
    }

    [Fact]
    public async void Should_Return_SuccessResult_When_Email_Is_Changed_Successfully()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var result = await _handler.Handle(GetValidCommand(validMember.Id), new CancellationToken());

        Assert.True(result.IsSuccess);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Email_Is_The_Same_As_Old_Email()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var result = await _handler.Handle(new ChangeEmailMemberCommand
        {
            MemberId = validMember.Id,
            Email = "test@test.se"
        }, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Email can not be the same as current email", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Email_Is_Null()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var result = await _handler.Handle(new ChangeEmailMemberCommand
        {
            MemberId = validMember.Id,
            Email = null
        }, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Value cannot be null. (Parameter 'email')", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_Member_Is_Not_Found()
    {
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns((Member?)null);

        var result = await _handler.Handle(GetValidCommand(Guid.NewGuid()), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No member found", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
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


        var result = await _handler.Handle(GetValidCommand(validMember.Id), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("MemberRepository failed", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
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


        var result = await _handler.Handle(GetValidCommand(validMember.Id), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("EventRepository failed", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
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


        var result = await _handler.Handle(GetValidCommand(validMember.Id), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("UnitOfWork failed", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Null()
    {
        var command = new ChangeEmailMemberCommand
        {
            MemberId = Guid.NewGuid(),
            Email = null
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Email);
    }

    [Fact]
    public void Should_Have_Error_When_MemberId_Is_Empty()
    {
        var command = new ChangeEmailMemberCommand
        {
            MemberId = Guid.Empty,
            Email = "Test@test.se"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.MemberId);
    }

    private static ChangeEmailMemberCommand GetValidCommand(Guid id)
    {
        return new ChangeEmailMemberCommand
        {
            MemberId = id,
            Email = "Mock@test.se"
        };
    }
}