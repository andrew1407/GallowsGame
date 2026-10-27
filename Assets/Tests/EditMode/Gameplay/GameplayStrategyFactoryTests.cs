using System;
using NUnit.Framework;

public class GameplayStrategyFactoryTests
{
    [Test]
    public void Should_Make_Offline_Strategy()
    {
        GameplayStrategyFactory factory = new();

        OfflineStrategy strategy = factory.MakeOfflineStrategy(new GameRules(new[] { "sasik" }));

        Assert.IsNotNull(strategy);
    }

    [Test]
    public void Should_Make_Http_Strategy_By_Uri_Scheme()
    {
        GameplayStrategyFactory factory = new();

        IGameplayStrategy strategy = factory.MakeOnlineStrategyByUri(new Uri("http://localhost:8080"));

        Assert.IsInstanceOf<HttpClientStrategy>(strategy);
        (strategy as IDisposable).Dispose();
    }

    [Test]
    public void Should_Make_WebSocket_Strategy_By_Uri_Scheme()
    {
        GameplayStrategyFactory factory = new();

        IGameplayStrategy strategy = factory.MakeOnlineStrategyByUri(new Uri("ws://localhost:8080"));

        Assert.IsInstanceOf<WebSocketClientStrategy>(strategy);
    }

    [Test]
    public void Should_Make_Tcp_Strategy_By_Uri_Scheme()
    {
        GameplayStrategyFactory factory = new();

        IGameplayStrategy strategy = factory.MakeOnlineStrategyByUri(new Uri("tcp://localhost:8080"));

        Assert.IsInstanceOf<TcpClientStrategy>(strategy);
        (strategy as IDisposable).Dispose();
    }

    [Test]
    public void Should_Make_Udp_Strategy_By_Uri_Scheme()
    {
        GameplayStrategyFactory factory = new();

        // Port 0 lets the OS pick a free local port, so the test doesn't clash with anything listening on 8080
        IGameplayStrategy strategy = factory.MakeOnlineStrategyByUri(new Uri("udp://localhost:8080?client_port=0"));

        Assert.IsInstanceOf<UdpClientStrategy>(strategy);
        (strategy as IDisposable).Dispose();
    }

    [Test]
    public void Should_Throw_Uri_Format_Exception_For_Unsupported_Scheme()
    {
        GameplayStrategyFactory factory = new();
        string[] addresses = { "https://localhost", "ftp://localhost", "file:///sasik" };

        foreach (var address in addresses)
            Assert.Throws<UriFormatException>(() => factory.MakeOnlineStrategyByUri(new Uri(address)), message: $"{address} shouldn't be supported");
    }
}
