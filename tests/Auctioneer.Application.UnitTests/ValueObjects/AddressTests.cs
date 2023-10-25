using Auctioneer.Application.ValueObjects;

namespace Auctioneer.Application.UnitTests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Same_Address_Should_Be_Equal()
    {
        var address1 = new Address("testgatan 2", "12345", "testholm");
        var address2 = new Address("testgatan 2", "12345", "testholm");

        Assert.Equal(address1, address2);
        Assert.True(EqualityComparer<Address>.Default.Equals(address1, address2));
        Assert.True(Equals(address1, address2));
        Assert.True(address1.Equals(address2));
        Assert.True(address1 == address2);
    }

    [Fact]
    public void Different_Address_Should_Not_Be_Equal()
    {
        var address1 = new Address("testgatan 2", "12345", "testholm");
        var address2 = new Address("testgatan 1", "12345", "testholm");

        Assert.NotEqual(address1, address2);
        Assert.False(EqualityComparer<Address>.Default.Equals(address1, address2));
        Assert.False(Equals(address1, address2));
        Assert.False(address1.Equals(address2));
        Assert.False(address1 == address2);
    }

    [Fact]
    public void Address_Should_Throw_ArgumentNullException_If_Street_Is_Null()
    {
        Action a = () => new Address(null, "12345", "testholm");
        Assert.Throws<ArgumentNullException>(a);
    }

    [Fact]
    public void Address_Should_Throw_ArgumentNullException_If_Zipcode_Is_Null()
    {
        Action a = () => new Address("Testgatan 2", null, "testholm");
        Assert.Throws<ArgumentNullException>(a);
    }

    [Fact]
    public void Address_Should_Throw_ArgumentNullException_If_City_Is_Null()
    {
        Action a = () => new Address("Testgatan 2", "12345", null);
        Assert.Throws<ArgumentNullException>(a);
    }
}