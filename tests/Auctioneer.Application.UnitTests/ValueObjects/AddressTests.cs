using Auctioneer.Application.ValueObjects;

namespace Auctioneer.Application.UnitTests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Same_Address_Should_Be_Equal()
    {
        var address1 = new Address("testgatan 2", "12345", "testholm");
        var address2 = new Address("testgatan 2", "12345", "testholm");

        address2.Should().Be(address1);
        EqualityComparer<Address>.Default.Equals(address1, address2).Should().BeTrue();
        Equals(address1, address2).Should().BeTrue();
        address1.Equals(address2).Should().BeTrue();
        (address1 == address2).Should().BeTrue();
    }

    [Fact]
    public void Different_Address_Should_Not_Be_Equal()
    {
        var address1 = new Address("testgatan 2", "12345", "testholm");
        var address2 = new Address("testgatan 1", "12345", "testholm");

        address2.Should().NotBe(address1);
        EqualityComparer<Address>.Default.Equals(address1, address2).Should().BeFalse();
        Equals(address1, address2).Should().BeFalse();
        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
    }

    [Fact]
    public void Address_Should_Throw_ArgumentNullException_If_Street_Is_Null()
    {
        Action a = () => new Address(null, "12345", "testholm");
        a.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Address_Should_Throw_ArgumentNullException_If_Zipcode_Is_Null()
    {
        Action a = () => new Address("Testgatan 2", null, "testholm");
        a.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Address_Should_Throw_ArgumentNullException_If_City_Is_Null()
    {
        Action a = () => new Address("Testgatan 2", "12345", null);
        a.Should().Throw<ArgumentNullException>();
    }
}