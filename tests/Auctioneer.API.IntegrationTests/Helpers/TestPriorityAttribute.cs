namespace Auctioneer.API.IntegrationTests.Helpers;

[AttributeUsage(AttributeTargets.Method)]
public class TestPriorityAttribute(int priority) : Attribute
{
    public int Priority { get; private set; } = priority;
}