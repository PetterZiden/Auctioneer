using Auctioneer.EmailService.Common;
using Auctioneer.EmailService.Interfaces;
using Auctioneer.MessagingContracts.Email;
using FluentEmail.Core;

namespace Auctioneer.EmailService.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public async Task CreateNewMemberMailMessage(CreateMemberMessage message)
    {
        try
        {
            var template = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                "../Templates/CreateMemberTemplate.html"));

            var email = Email
                .From(Consts.EmailFrom)
                .To(message.Email)
                .Subject(Consts.CreateMemberSubject)
                .UsingTemplate(template, new { Name = message.FirstName + " " + message.LastName });

            await email.SendAsync();

            logger.LogInformation("Create Member Email Sent to {Email}", message.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong trying to send email");
            throw;
        }
    }

    public async Task CreateNewAuctionEmail(CreateAuctionMessage message)
    {
        try
        {
            var template = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                "../Templates/CreateAuctionTemplate.html"));

            var email = Email
                .From(Consts.EmailFrom)
                .To(message.Email)
                .Subject(Consts.CreateAuctionSubject)
                .UsingTemplate(template,
                    new { Name = message.MemberName, message.Title, Price = message.StartingPrice });

            await email.SendAsync();

            logger.LogInformation("Create Auction Email Sent to {Email}", message.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong trying to send email");
            throw;
        }
    }

    public async Task PlaceBidEmail(PlaceBidMessage message)
    {
        try
        {
            var template =
                await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                    "../Templates/PlaceBidTemplate.html"));

            var email = Email
                .From(Consts.EmailFrom)
                .To(message.BidderEmail)
                .Subject(Consts.PlaceBidSubject)
                .UsingTemplate(template,
                    new { Name = message.BidderName, Title = message.AuctionTitle, BidPrice = message.Bid });

            await email.SendAsync();

            logger.LogInformation("Place Bid Email Sent to {Email}", message.BidderEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong trying to send email");
            throw;
        }
    }

    public async Task RateMemberEmail(RateMemberMessage message)
    {
        try
        {
            var template = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory,
                "../Templates/RateMemberTemplate.html"));

            var email = Email
                .From(Consts.EmailFrom)
                .To(message.RatedEmail)
                .Subject(Consts.RateMemberSubject)
                .UsingTemplate(template, new { Name = message.RatedName, RatedBy = message.RatedByName });

            await email.SendAsync();

            logger.LogInformation("Rate Member Email Sent to {Email}", message.RatedEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong trying to send email");
            throw;
        }
    }
}