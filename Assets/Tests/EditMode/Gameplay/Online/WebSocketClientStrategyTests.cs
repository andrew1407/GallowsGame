using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WebSocketSharp;
using WebSocketSharp.Server;

public class WebSocketClientStrategyTests
{
    private const string _path = "/gallows";

    private WebSocketServer _server;

    private string _address;

    [SetUp]
    public void SetUp()
    {
        GameService.Reset();
        int port = ProtocolMessages.FreePort();
        _address = $"ws://127.0.0.1:{port}{_path}";
        _server = new WebSocketServer($"ws://127.0.0.1:{port}");
        _server.AddWebSocketService<GameService>(_path);
        _server.Start();
    }

    [TearDown]
    public void TearDown() => _server.Stop();

    [Test]
    public void Should_Return_Greeting_Without_Sending_Input()
    {
        WebSocketClientStrategy strategy = new(_address);
        GameService.Greeting = ProtocolMessages.Progress(new() { Stage = GameStageLabels.Difficulty });

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        Assert.AreEqual(GameStageLabels.Difficulty, actual.Stage);
        Assert.IsEmpty(GameService.Requests, message: "The first stage comes from the server on connection");
    }

    [Test]
    public void Should_Send_Input_And_Return_Response()
    {
        WebSocketClientStrategy strategy = new(_address);
        GameService.Greeting = ProtocolMessages.Progress(new() { Stage = GameStageLabels.Difficulty });
        GameService.Responses.Enqueue(ProtocolMessages.Progress(new() { Stage = GameStageLabels.Gameplay, Tries = 7 }));

        Sync.Run(strategy.Setup);
        Sync.Run(() => strategy.StageAction(string.Empty));
        GameProgress actual = Sync.Run(() => strategy.StageAction("2"));
        strategy.Dispose();

        Assert.AreEqual(GameStageLabels.Gameplay, actual.Stage);
        Assert.AreEqual(7, actual.Tries);
        Assert.True(GameService.Requests.TryDequeue(out JObject request), message: "Input should be sent to the server");
        Assert.AreEqual(RequestPayload.EVENT_NAME, request["event"].Value<string>());
        Assert.AreEqual("2", request["data"]["input"].Value<string>());
    }

    [Test]
    public void Should_Return_Default_Progress_On_End_Response()
    {
        WebSocketClientStrategy strategy = new(_address);
        GameService.Greeting = ProtocolMessages.End();

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Return_Default_Progress_On_Other_Event()
    {
        WebSocketClientStrategy strategy = new(_address);
        GameService.Greeting = ProtocolMessages.OtherEvent(new() { Stage = GameStageLabels.Difficulty });

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Return_Default_Progress_After_Dispose()
    {
        WebSocketClientStrategy strategy = new(_address);
        GameService.Greeting = ProtocolMessages.Progress(new() { Stage = GameStageLabels.Difficulty });

        Sync.Run(strategy.Setup);
        strategy.Dispose();
        GameProgress actual = Sync.Run(() => strategy.StageAction("b"));

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Return_Default_Progress_Before_Setup()
    {
        WebSocketClientStrategy strategy = new(_address);

        GameProgress actual = Sync.Run(() => strategy.StageAction("b"));

        Assert.AreEqual(default(GameProgress), actual);
    }

    // Like the real server: greets every connection, then answers each message
    private class GameService : WebSocketBehavior
    {
        public static string Greeting;

        public static ConcurrentQueue<string> Responses;

        public static ConcurrentQueue<JObject> Requests;

        public static void Reset()
        {
            Greeting = "{}";
            Responses = new();
            Requests = new();
        }

        protected override void OnOpen() => Send(Greeting);

        protected override void OnMessage(MessageEventArgs e)
        {
            Requests.Enqueue(JObject.Parse(e.Data));
            Send(Responses.TryDequeue(out string response) ? response : "{}");
        }
    }
}
