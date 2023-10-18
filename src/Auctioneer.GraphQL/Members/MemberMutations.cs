using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Commands;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.GraphQL.Members.Inputs;
using Auctioneer.GraphQL.Members.Payloads;
using HotChocolate.Execution;

namespace Auctioneer.GraphQL.Members;

[ExtendObjectType("Mutation")]
public class MemberMutations
{
    public async Task<CreateMemberPayload> CreateMember(CreateMemberInput input,
        [Service] IRepository<Member> memberRepository, [Service] IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateMemberCommand
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                Street = input.Street,
                ZipCode = input.ZipCode,
                City = input.City,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber
            };

            var validationResult = await new CreateMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(string.Join(Environment.NewLine, errorMessages))
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            var member = Member.Create(
                input.FirstName,
                input.LastName,
                input.Email,
                input.PhoneNumber,
                input.Street,
                input.ZipCode,
                input.City
            );

            var domainEvent = new MemberCreatedEvent(member, EventList.Member.MemberCreatedEvent);

            await memberRepository.CreateAsync(member, cancellationToken);
            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await unitOfWork.SaveAsync();

            return new CreateMemberPayload(member.Id, member.Created);
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

    public async Task<UpdateMemberPayload> UpdateMember(UpdateMemberInput input,
        [Service] IRepository<Member> memberRepository, [Service] IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateMemberCommand
            {
                Id = input.MemberId,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Street = input.Street,
                ZipCode = input.ZipCode,
                City = input.City,
                PhoneNumber = input.PhoneNumber
            };

            var validationResult = await new UpdateMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(string.Join(Environment.NewLine, errorMessages))
                        .SetCode("BAD_REQUEST")
                        .Build());
            }


            var member = await memberRepository.GetAsync(command.Id);

            if (member is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            if (!string.IsNullOrEmpty(command.FirstName))
            {
                var result = member.ChangeFirstName(command.FirstName);
                if (!result.IsSuccess)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage(result.Errors[0].Message)
                            .SetCode("BAD_REQUEST")
                            .Build());
                }
            }

            if (!string.IsNullOrEmpty(command.LastName))
            {
                var result = member.ChangeLastName(command.LastName);
                if (!result.IsSuccess)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage(result.Errors[0].Message)
                            .SetCode("BAD_REQUEST")
                            .Build());
                }
            }

            if (!string.IsNullOrEmpty(command.PhoneNumber))
            {
                var result = member.ChangePhoneNumber(command.PhoneNumber);
                if (!result.IsSuccess)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage(result.Errors[0].Message)
                            .SetCode("BAD_REQUEST")
                            .Build());
                }
            }

            if (!string.IsNullOrEmpty(command.Street) || !string.IsNullOrEmpty(command.ZipCode) ||
                !string.IsNullOrEmpty(command.City))
            {
                var addressToUpdate = new Address
                {
                    Street = command.Street ?? member.Address.Street,
                    ZipCode = command.ZipCode ?? member.Address.ZipCode,
                    City = command.City ?? member.Address.City
                };
                var result = member.ChangeAddress(addressToUpdate);
                if (!result.IsSuccess)
                {
                    throw new QueryException(
                        ErrorBuilder.New()
                            .SetMessage(result.Errors[0].Message)
                            .SetCode("BAD_REQUEST")
                            .Build());
                }
            }

            var domainEvent = new MemberUpdatedEvent(member, EventList.Member.MemberUpdatedEvent);

            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await memberRepository.UpdateAsync(command.Id, member, cancellationToken);
            await unitOfWork.SaveAsync();

            return new UpdateMemberPayload(member.Id, member.LastModified.Value);
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

    public async Task<DeleteMemberPayload> DeleteMember(DeleteMemberInput input,
        [Service] IRepository<Member> memberRepository, [Service] IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var member = await memberRepository.GetAsync(input.MemberId);
            if (member is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }


            var domainEvent = new MemberDeletedEvent(input.MemberId, EventList.Member.MemberDeletedEvent);

            await memberRepository.DeleteAsync(input.MemberId, cancellationToken);
            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await unitOfWork.SaveAsync();

            return new DeleteMemberPayload(input.MemberId, "Member deleted successfully");
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

    public async Task<ChangeEmailPayload> ChangeEmail(ChangeEmailInput input,
        [Service] IRepository<Member> memberRepository, IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ChangeEmailMemberCommand { MemberId = input.MemberId, Email = input.Email };

            var validationResult =
                await new ChangeEmailMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(string.Join(Environment.NewLine, errorMessages))
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            var member = await memberRepository.GetAsync(command.MemberId);

            if (member is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            var result = member.ChangeEmail(command.Email);

            if (!result.IsSuccess)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(result.Errors[0].Message)
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            var domainEvent =
                new MemberChangedEmailEvent(member.Id, command.Email, EventList.Member.MemberChangedEmailEvent);

            await memberRepository.UpdateAsync(member.Id, member, cancellationToken);
            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await unitOfWork.SaveAsync();

            return new ChangeEmailPayload($"Email for member with id: {member.Id} changed successfully");
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

    public async Task<RateMemberPayload> RateMember(RateMemberInput input,
        [Service] IRepository<Member> memberRepository, IRepository<DomainEvent> eventRepository,
        [Service] IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        try
        {
            var command = new RateMemberCommand
            {
                RatingForMemberId = input.RatingForMemberId,
                RatingFromMemberId = input.RatingFromMemberId,
                Stars = input.Stars
            };

            var validationResult = await new RateMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(string.Join(Environment.NewLine, errorMessages))
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            var ratedMember = await memberRepository.GetAsync(command.RatingForMemberId);
            var ratedByMember = await memberRepository.GetAsync(command.RatingFromMemberId);

            if (ratedMember is null || ratedByMember is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            var result = ratedMember.Rate(command.RatingFromMemberId, command.Stars);

            if (!result.IsSuccess)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(result.Errors[0].Message)
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            var rateMemberDto = new RateMemberDto
            {
                RatedName = ratedMember.FullName,
                RatedMemberId = ratedMember.Id,
                RatedEmail = ratedMember.Email,
                RatedByName = ratedByMember.FullName,
                Stars = command.Stars
            };
            var domainEvent = new RateMemberEvent(rateMemberDto, EventList.Member.RateMemberEvent);

            await memberRepository.UpdateAsync(ratedMember.Id, ratedMember, cancellationToken);
            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await unitOfWork.SaveAsync();

            return new RateMemberPayload("Member rated successfully");
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