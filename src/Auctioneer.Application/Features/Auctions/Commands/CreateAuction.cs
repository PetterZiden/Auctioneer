using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Contracts;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Commands;

public class CreateAuctionController : ApiControllerBase
{
    private readonly ILogger<CreateAuctionController> _logger;

    public CreateAuctionController(ILogger<CreateAuctionController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpPost("api/auction")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> Create(CreateAuctionRequest request)
    {
        try
        {
            var command = new CreateAuctionCommand
            {
                MemberId = request.MemberId,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                StartingPrice = request.StartingPrice,
                ImgRoute = request.ImgRoute
            };

            var validationResult = await new CreateAuctionCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                return Ok(result.Value);

            return ReturnError(result.Errors.FirstOrDefault() as Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
    }
}

public class CreateAuctionCommand : IRequest<Result<Guid>>
{
    public Guid MemberId { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public decimal StartingPrice { get; init; }
    public string ImgRoute { get; init; }
}

internal sealed class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, Result<Guid>>
{
    private readonly IRepository<Auction> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAuctionCommandHandler(IRepository<Auction> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = Auction.Create(
                request.MemberId,
                request.Title,
                request.Description,
                request.StartTime,
                request.EndTime,
                request.StartingPrice,
                request.ImgRoute
            );

            await _repository.CreateAsync(auction);
            await _unitOfWork.SaveAsync();

            return Result.Ok(auction.Id);
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        //Todo: fixa all validering
        RuleFor(v => v.Title)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Description)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.StartingPrice)
            .GreaterThan(0);
    }
}