using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Commands;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Auctioneer.Application.UnitTests.Features.Members.Commands;

public class UpdateMemberTests
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateMemberCommandHandler _handler;
    private readonly UpdateMemberCommandValidator _validator = new();

    public UpdateMemberTests()
    {
        _memberRepository = Substitute.For<IRepository<Member>>();
        _eventRepository = Substitute.For<IRepository<DomainEvent>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateMemberCommandHandler(_memberRepository, _eventRepository, _unitOfWork);
    }

    [Fact]
    public async void Should_Return_SuccessResult_When_Member_Is_Updated_Successfully()
    {
        var validMember = GetValid.Member();

        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);
        _memberRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsSuccess);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
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


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("MemberRepository failed", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
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

        Assert.True(result.IsFailed);
        Assert.Equal("EventRepository failed", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(0)
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

        Assert.True(result.IsFailed);
        Assert.Equal("UnitOfWork failed", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<Guid>());
        await _memberRepository.Received(1)
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new UpdateMemberCommand
        {
            Id = Guid.Empty,
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 2",
            Zipcode = "12345",
            City = "Testholm",
            PhoneNumber = "0732223311"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Id);
    }


    private static UpdateMemberCommand GetValidCommand()
    {
        return new UpdateMemberCommand
        {
            Id = Guid.NewGuid(),
            FirstName = "Test22",
            LastName = "Testsson22",
            Street = "Testgatan 222",
            Zipcode = "12341",
            City = "Testborg",
            PhoneNumber = "0734443311"
        };
    }
}