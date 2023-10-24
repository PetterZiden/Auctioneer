using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Common.Validators;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Commands;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.GraphQL.Auctions.Inputs;
using Auctioneer.GraphQL.Auctions.Payloads;
using HotChocolate.Execution;

namespace Auctioneer.GraphQL.Auctions;

[ExtendObjectType("Mutation")]
public class AuctionMutations
{
    public async Task<CreateAuctionPayload> CreateAuction(CreateAuctionInput input,
        [Service] IRepository<Auction> auctionRepository, [Service] IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateAuctionCommand
            {
                MemberId = input.MemberId,
                Title = input.Title,
                Description = input.Description,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
                StartingPrice = input.StartingPrice,
                ImgRoute = input.ImgRoute
            };

            var validationResult = await new CreateAuctionCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(string.Join(Environment.NewLine, errorMessages))
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            var auction = Auction.Create(
                input.MemberId,
                input.Title,
                input.Description,
                input.StartTime,
                input.EndTime,
                input.StartingPrice,
                input.ImgRoute
            );

            var domainEvent = new AuctionCreatedEvent(auction, EventList.Auction.AuctionCreatedEvent);

            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await auctionRepository.CreateAsync(auction, cancellationToken);
            await unitOfWork.SaveAsync();

            return new CreateAuctionPayload(auction.Id, auction.Created);
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<UpdateAuctionPayload> UpdateAuction(UpdateAuctionInput input,
        [Service] IRepository<Auction> auctionRepository, [Service] IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateAuctionCommand
            {
                Id = input.AuctionId,
                Title = input.Title,
                Description = input.Description,
                ImgRoute = input.ImgRoute
            };

            var validationResult = await new UpdateAuctionCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                ErrorBuilder.New()
                    .SetMessage(string.Join(Environment.NewLine, errorMessages))
                    .SetCode("BAD_REQUEST")
                    .Build();
            }

            var auction = await auctionRepository.GetAsync(input.AuctionId);
            if (auction is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No auction found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            if (!string.IsNullOrEmpty(command.Title))
            {
                var result = auction.ChangeTitle(command.Title);
                if (!result.IsSuccess)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage(result.Errors[0].Message)
                            .SetCode("BAD_REQUEST")
                            .Build());
                }
            }

            if (!string.IsNullOrEmpty(command.Description))
            {
                var result = auction.ChangeDescription(command.Description);
                if (!result.IsSuccess)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage(result.Errors[0].Message)
                            .SetCode("BAD_REQUEST")
                            .Build());
                }
            }

            if (!string.IsNullOrEmpty(command.ImgRoute))
            {
                var result = auction.ChangeImageRoute(command.ImgRoute);
                if (!result.IsSuccess)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage(result.Errors[0].Message)
                            .SetCode("BAD_REQUEST")
                            .Build());
                }
            }

            var domainEvent = new AuctionUpdatedEvent(auction, EventList.Auction.AuctionUpdatedEvent);

            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await auctionRepository.UpdateAsync(command.Id, auction, cancellationToken);
            await unitOfWork.SaveAsync();

            return new UpdateAuctionPayload(auction.Id, auction.LastModified!.Value);
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<DeleteAuctionPayload> DeleteAuction(DeleteAuctionInput input,
        [Service] IRepository<Auction> auctionRepository, [Service] IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await auctionRepository.GetAsync(input.AuctionId);
            if (auction is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No auction found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            var domainEvent = new AuctionDeletedEvent(input.AuctionId, EventList.Auction.AuctionDeletedEvent);

            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await auctionRepository.DeleteAsync(input.AuctionId, cancellationToken);
            await unitOfWork.SaveAsync();

            return new DeleteAuctionPayload(input.AuctionId, "Auction deleted successfully");
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<PlaceBidPayload> PlaceBid(PlaceBidInput input, [Service] IRepository<Auction> auctionRepository,
        [Service] IRepository<Member> memberRepository, [Service] IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var command = new Bid
            {
                AuctionId = input.AuctionId,
                MemberId = input.MemberId,
                BidPrice = input.BidPrice
            };

            var validationResult = await new BidValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                ErrorBuilder.New()
                    .SetMessage(string.Join(Environment.NewLine, errorMessages))
                    .SetCode("BAD_REQUEST")
                    .Build();
            }

            var auction = await auctionRepository.GetAsync(input.AuctionId);
            if (auction is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No auction found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            var bidResult = auction.PlaceBid(input.MemberId, input.BidPrice);
            if (!bidResult.IsSuccess)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(bidResult.Errors?.FirstOrDefault()?.Message!)
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            var bidder = await memberRepository.GetAsync(input.MemberId);
            var auctionOwner = await memberRepository.GetAsync(auction.MemberId);
            if (bidder is null || auctionOwner is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOND")
                        .Build());
            }

            var memberResult = bidder.AddBid(bidResult.Value.AuctionId, bidResult.Value.BidPrice,
                bidResult.Value.TimeStamp!.Value);
            if (!memberResult.IsSuccess)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(memberResult.Errors?.FirstOrDefault()?.Message!)
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            var placeBidDto = new PlaceBidDto
            {
                AuctionOwnerId = auctionOwner.Id,
                AuctionTitle = auction.Title,
                AuctionOwnerName = auctionOwner.FullName,
                AuctionOwnerEmail = auctionOwner.Email,
                Bid = command.BidPrice,
                BidderName = bidder.FullName,
                BidderEmail = bidder.Email,
                TimeStamp = bidResult.Value.TimeStamp.Value,
                AuctionUrl = $"https://localhost:7298/api/auction/{auction.Id}"
            };
            var domainEvent = new AuctionPlaceBidEvent(placeBidDto, EventList.Auction.AuctionPlaceBidEvent);

            await auctionRepository.UpdateAsync(auction.Id, auction, cancellationToken);
            await memberRepository.UpdateAsync(bidder.Id, bidder, cancellationToken);
            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await unitOfWork.SaveAsync();

            return new PlaceBidPayload($"Placed bid successfully on auction with id: {auction.Id}");
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }
}