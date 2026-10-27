using NUnit.Framework;

public class LinkAddressActionTests
{
    [Test]
    public void Should_Accept_Http_And_Https_Urls()
    {
        string[] urls = { "http://github.com", "https://github.com/andrew1407/GallowsGame", "https://localhost:8080/path?q=1" };

        foreach (var url in urls)
            Assert.True(LinkAddressAction.IsValidURL(url), message: $"{url} should be valid");
    }

    [Test]
    public void Should_Reject_Other_Schemes_And_Invalid_Values()
    {
        string[] urls = { null, string.Empty, "sasik", "github.com", "/relative/path", "ftp://github.com", "ws://localhost:8080", "file:///etc/passwd", "javascript:alert(1)" };

        foreach (var url in urls)
            Assert.False(LinkAddressAction.IsValidURL(url), message: $"{url} shouldn't be valid");
    }
}
