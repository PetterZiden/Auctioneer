using System.ComponentModel.DataAnnotations;
using Auctioneer.Application.ValueObjects;

namespace Auctioneer.Application.UnitTests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Same_Email_Should_Be_Equal()
    {
        var email1 = new Email("test@test.se");
        var email2 = new Email("test@test.se");

        email2.Should().Be(email1);
        EqualityComparer<Email>.Default.Equals(email1, email2).Should().BeTrue();
        Equals(email1, email2).Should().BeTrue();
        email1.Equals(email2).Should().BeTrue();
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Different_Email_Should_Not_Be_Equal()
    {
        var email1 = new Email("test@test.se");
        var email2 = new Email("test1@test.se");

        email2.Should().NotBe(email1);
        EqualityComparer<Email>.Default.Equals(email1, email2).Should().BeFalse();
        Equals(email1, email2).Should().BeFalse();
        email1.Equals(email2).Should().BeFalse();
        (email1 == email2).Should().BeFalse();
    }

    [Fact]
    public void Email_Should_Throw_ValidationException_If_Email_Is_Not_Valid_Format()
    {
        Action a = () => new Email("test.se");
        a.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Email_Should_Throw_ArgumentNullException_If_Email_Is_Null()
    {
        Action a = () => new Email(null);
        a.Should().Throw<ArgumentNullException>();
    }
}