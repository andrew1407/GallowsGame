using System;
using System.Collections.Generic;
using System.Net.Http;

public class GameplayStrategyFactory
{
    public const string HTTP_LABEL = "http";

    public const string WS_LABEL = "ws";

    public const string UDP_LABEL = "udp";

    public const string TCP_LABEL = "tcp";

    private readonly Dictionary<string, Func<Uri, IGameplayStrategy>> _onlineStrategies;

    public GameplayStrategyFactory()
    {
        _onlineStrategies = new() {
            {HTTP_LABEL, MakeHttpStrategy},
            {WS_LABEL, MakeWsStrategy},
            {UDP_LABEL, MakeUdpStrategy},
            {TCP_LABEL, MakeTcpStrategy},
        };
    }

    public OfflineStrategy MakeOfflineStrategy(GameRules gameRules) => new(gameRules);

    public HttpClientStrategy MakeHttpStrategy(Uri uri) => new(new HttpClient(), uri);

    public WebSocketClientStrategy MakeWsStrategy(Uri uri) => new(uri);

    public UdpClientStrategy MakeUdpStrategy(Uri uri) => UdpClientStrategy.Of(uri);

    public TcpClientStrategy MakeTcpStrategy(Uri uri) => new(uri);

    public IGameplayStrategy MakeOnlineStrategyByUri(Uri uri)
    {
        string scheme = uri.Scheme;
        if (_onlineStrategies.ContainsKey(scheme)) return _onlineStrategies[scheme](uri);
        throw new UriFormatException();
    }
}

