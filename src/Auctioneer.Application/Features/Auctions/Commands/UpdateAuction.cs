using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Commands;

public class UpdateAuctionController : ApiControllerBase
{
    private readonly ILogger<UpdateAuctionController> _logger;

    public UpdateAuctionController(ILogger<UpdateAuctionController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpPut("api/auction")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> Update(AuctionDto auction)
    {
        try
        {
            var command = new UpdateAuctionCommand { Auction = auction };

            var validationResult = await new UpdateAuctionCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                return Ok();

            return ReturnError(result.Errors.FirstOrDefault() as Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
    }
}

public class UpdateAuctionCommand : IRequest<Result>
{
    public AuctionDto Auction { get; init; }
}

internal sealed class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand, Result>
{
    private readonly IRepository<Auction> _repository;

    public UpdateAuctionCommandHandler(IRepository<Auction> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Auction.Id is null)
                return Result.Fail(new Error("Auction id is required"));

            var auction = await _repository.GetAsync(request.Auction.Id.Value);

            if (auction is null)
                return Result.Fail(new Error("No auction found"));

            await _repository.UpdateAsync(request.Auction.Id.Value, auction);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    public UpdateAuctionCommandValidator()
    {
        //Todo: fixa all validering
        RuleFor(v => v.Auction.Id)
            .NotNull();

        RuleFor(v => v.Auction.Title)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Auction.Description)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Auction.StartingPrice)
            .GreaterThan(0);
    }
}