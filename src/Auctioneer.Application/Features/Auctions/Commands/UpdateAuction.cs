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
    public async Task<ActionResult<Guid>> Update(UpdateAuctionRequest request)
    {
        try
        {
            var command = new UpdateAuctionCommand
            {
                Id = request.AuctionId,
                Title = request.Title,
                Description = request.Description,
                ImgRoute = null
            };

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
    public Guid Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string ImgRoute { get; init; }
}

internal sealed class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand, Result>
{
    private readonly IRepository<Auction> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAuctionCommandHandler(IRepository<Auction> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await _repository.GetAsync(request.Id);

            if (auction is null)
                return Result.Fail(new Error("No auction found"));

            await _repository.UpdateAsync(request.Id, auction);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    public UpdateAuctionCommandValidator()
    {
        //Todo: fixa all validering
        RuleFor(v => v.Id)
            .NotNull();

        RuleFor(v => v.Title)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Description)
            .NotNull()
            .NotEmpty();
    }
}