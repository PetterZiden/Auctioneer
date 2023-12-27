using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Commands;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Auctioneer.Application.UnitTests.Features.Members.Commands;

public class CreateMemberTests
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateMemberCommandHandler _handler;
    private readonly CreateMemberCommandValidator _validator = new();

    public CreateMemberTests()
    {
        _memberRepository = Substitute.For<IRepository<Member>>();
        _eventRepository = Substitute.For<IRepository<DomainEvent>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateMemberCommandHandler(_memberRepository, _eventRepository, _unitOfWork);
    }

    [Fact]
    public async void Should_Return_SuccessResult_And_MemberId_When_Member_Is_Created_Successfully()
    {
        _memberRepository.CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.IsType<Guid>(result.Value);
        await _memberRepository.Received(1).CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_FirstName_Is_Null()
    {
        var request = new CreateMemberCommand
        {
            FirstName = null,
            LastName = "Testsson",
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0732223311"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("First name can not be empty", result.Errors[0].Message);
        await _memberRepository.Received(0).CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_LastName_Is_Null()
    {
        var request = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = null,
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0732223311"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Last name can not be empty", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_Email_Is_Null()
    {
        var request = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = "Testholm",
            Email = null,
            PhoneNumber = "0732223311"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Value cannot be null. (Parameter 'email')", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_PhoneNumber_Is_Null()
    {
        var request = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = null
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Phone number can not be empty", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_Street_Is_Null()
    {
        var request = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = null,
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0732223311"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Value cannot be null. (Parameter 'street')", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_Zipcode_Is_Null()
    {
        var request = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 2",
            ZipCode = null,
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0732223311"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Value cannot be null. (Parameter 'zipcode')", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_With_ErrorMsg_When_City_Is_Null()
    {
        var request = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = null,
            Email = "Test@test.se",
            PhoneNumber = "0732223311"
        };

        var result = await _handler.Handle(request, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("Value cannot be null. (Parameter 'city')", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        _memberRepository.CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("MemberRepository failed"));
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("MemberRepository failed", result.Errors[0].Message);
        await _memberRepository.Received(1).CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(0).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_EventRepository_Throws_Exception()
    {
        _memberRepository.CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new Exception("EventRepository failed"));
        _unitOfWork.SaveAsync().Returns(Task.CompletedTask);


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("EventRepository failed", result.Errors[0].Message);
        await _memberRepository.Received(1).CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(0).SaveAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_UnitOfWork_Throws_Exception()
    {
        _memberRepository.CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _eventRepository.CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(x => throw new Exception("UnitOfWork failed"));


        var result = await _handler.Handle(GetValidCommand(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("UnitOfWork failed", result.Errors[0].Message);
        await _memberRepository.Received(1).CreateAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _eventRepository.Received(1).CreateAsync(Arg.Any<DomainEvent>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public void Should_Have_Error_When_FirstName_Is_Null()
    {
        var command = new CreateMemberCommand
        {
            FirstName = null,
            LastName = "Testsson",
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0732223311"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.FirstName);
    }

    [Fact]
    public void Should_Have_Error_When_LastName_Is_Null()
    {
        var command = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = null,
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0732223311"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.LastName);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        var command = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = "Testholm",
            Email = null,
            PhoneNumber = "0732223311"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(m => m.Email);
    }

    private static CreateMemberCommand GetValidCommand()
    {
        return new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 2",
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0732223311"
        };
    }
}