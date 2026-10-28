using System;
using System.Collections.Generic;
using System.Net.Http;

public class GameplayStrategyFactory
{
    public const string HTTP_LABEL = "http";

    public const string HTTPS_LABEL = "https";

    public const string WS_LABEL = "ws";

    public const string WSS_LABEL = "wss";

    public const string UDP_LABEL = "udp";

    public const string TCP_LABEL = "tcp";

    private readonly Dictionary<string, Func<Uri, IGameplayStrategy>> _onlineStrategies;

    public GameplayStrategyFactory()
    {
        // Secure schemes matter in the browser: an https page may only reach https/wss servers (or localhost)
        _onlineStrategies = new() {
            {HTTP_LABEL, MakeHttpStrategy},
            {HTTPS_LABEL, MakeHttpStrategy},
            {WS_LABEL, MakeWsStrategy},
            {WSS_LABEL, MakeWsStrategy},
        };
#if !UNITY_WEBGL || UNITY_EDITOR
        // Browsers allow no raw sockets, so UDP and TCP stay desktop only
        _onlineStrategies[UDP_LABEL] = MakeUdpStrategy;
        _onlineStrategies[TCP_LABEL] = MakeTcpStrategy;
#endif
    }

    public OfflineStrategy MakeOfflineStrategy(GameRules gameRules) => new(gameRules);

#if UNITY_WEBGL && !UNITY_EDITOR
    public HttpClientStrategy MakeHttpStrategy(Uri uri) => new(new WebRequestPoster(), uri);
#else
    public HttpClientStrategy MakeHttpStrategy(Uri uri) => new(new HttpClient(), uri);
#endif

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
