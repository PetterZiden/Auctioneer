using Auctioneer.Application;
using Auctioneer.GraphQL.GraphQL;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auctioneer.GraphQL.Tests;

public class MutationTest
{
    private readonly IRequestExecutorBuilder _graphQlServer = new ServiceCollection()
        .AddApplication()
        .AddGraphQLServer()
        .AddQueryType<Query>()
        .AddMutationType<Mutation>();

    //private readonly IConfiguration _configuration;

    // var builder = new ConfigurationBuilder()
    //     .SetBasePath(AppContext.BaseDirectory)
    //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //     .AddJsonFile("../Auctioneer.GraphQL/appsettings.json", optional: true)
    //     .AddUserSecrets(Assembly.GetEntryAssembly()!);
    //
    // _configuration = builder.Build();


    [Fact]
    public async void GetMemberReturnsSuccess()
    {
        // Arrange                                                                      
        var query = @"query{
                            members{
                                 id,
                                 lastName,
                                 firstName,
                                  street,
                                  zipCode,
                                  city,
                                  currentRating,
                                  numberOfRatings,
                                  bids {
                                    auctionId,
                                    memberId,
                                    bidPrice,
                                    timeStamp
                                  }
                            }
                        }";

        var request = QueryRequestBuilder.New()
            .SetQuery(query)
            .Create();
        // Act                                                                          
        var response = await _graphQlServer.ExecuteRequestAsync(request);

        // Assert                                                                       
        Assert.Null(response);
    }

    [Fact]
    public async void CreateMemberReturnsSuccess()
    {
        // Arrange
        var query = @"mutation createMember {
                          createMember(request: 
                            { 
                                firstName: ""Petter""
                                lastName: ""Test""
                                street: ""Skolvägen 2""
                                zipCode: ""12345""
                                city: ""Mockholm""
                                email: ""mock@test.se""
                                phoneNumber: ""07343543534""
                            }) {
                            record {
                              id
                              name
                            }
                            error
                          }
                        }";

        var request = QueryRequestBuilder.New()
            .SetQuery(query)
            .Create();
        // Act
        var response = await _graphQlServer.ExecuteRequestAsync(request);

        // Assert
        Assert.Null(response);
    }
}