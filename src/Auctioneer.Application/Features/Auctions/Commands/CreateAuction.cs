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
    public async Task<ActionResult<Guid>> Create(AuctionDto auction)
    {
        try
        {
            var command = new CreateAuctionCommand { Auction = auction };
            
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
    public AuctionDto Auction { get; init; }
}

internal sealed class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, Result<Guid>>
{
    private readonly IRepository<Auction> _repository;

    public CreateAuctionCommandHandler(IRepository<Auction> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = Auction.Create(
                request.Auction.MemberId,
                request.Auction.Title,
                request.Auction.Description,
                request.Auction.StartTime,
                request.Auction.EndTime,
                request.Auction.StartingPrice,
                request.Auction.ImgRoute
            );
            
            await _repository.CreateAsync(auction);

            return Result.Ok(auction.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        //Todo: fixa all validering
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