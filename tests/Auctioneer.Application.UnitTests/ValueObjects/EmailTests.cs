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

        Assert.Equal(email1, email2);
        Assert.True(EqualityComparer<Email>.Default.Equals(email1, email2));
        Assert.True(Equals(email1, email2));
        Assert.True(email1.Equals(email2));
        Assert.True(email1 == email2);
    }

    [Fact]
    public void Different_Email_Should_Not_Be_Equal()
    {
        var email1 = new Email("test@test.se");
        var email2 = new Email("test1@test.se");

        Assert.NotEqual(email1, email2);
        Assert.False(EqualityComparer<Email>.Default.Equals(email1, email2));
        Assert.False(Equals(email1, email2));
        Assert.False(email1.Equals(email2));
        Assert.False(email1 == email2);
    }

    [Fact]
    public void Email_Should_Throw_ValidationException_If_Email_Is_Not_Valid_Format()
    {
        Action a = () => new Email("test.se");
        Assert.Throws<ValidationException>(a);
    }

    [Fact]
    public void Email_Should_Throw_ArgumentNullException_If_Email_Is_Null()
    {
        Action a = () => new Email(null);
        Assert.Throws<ArgumentNullException>(a);
    }
}