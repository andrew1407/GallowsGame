using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class UdpClientStrategyTests
{
    private UdpClient _server;

    private ConcurrentQueue<string> _responses;

    private ConcurrentQueue<JObject> _requests;

    private int _port => ((IPEndPoint)_server.Client.LocalEndPoint).Port;

    [SetUp]
    public void SetUp()
    {
        _server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        _responses = new();
        _requests = new();
        Task.Run(serve);
    }

    [TearDown]
    public void TearDown() => _server.Dispose();

    [Test]
    public void Should_Send_Initial_Request_And_Remember_Id()
    {
        UdpClientStrategy strategy = makeTestable();
        _responses.Enqueue(ProtocolMessages.Progress(new() { Id = "42", Stage = GameStageLabels.Difficulty }));
        _responses.Enqueue(ProtocolMessages.Progress(new() { Id = "42", Stage = GameStageLabels.Gameplay, Tries = 13 }));

        Sync.Run(strategy.Setup);
        GameProgress first = Sync.Run(() => strategy.StageAction(string.Empty));
        GameProgress second = Sync.Run(() => strategy.StageAction("3"));
        strategy.Dispose();

        JObject[] requests = _requests.ToArray();
        Assert.AreEqual(2, requests.Length);
        Assert.AreEqual(RequestPayload.EVENT_NAME, requests[0]["event"].Value<string>());
        Assert.True(requests[0]["data"]["initial"].Value<bool>(), message: "First request should be initial");
        Assert.False(requests[0].ContainsKey("id"), message: "First request cannot have an id");
        Assert.AreEqual("42", requests[1]["id"].Value<string>(), message: "Id from the first response should be reused");
        Assert.AreEqual("3", requests[1]["data"]["input"].Value<string>());

        Assert.AreEqual(GameStageLabels.Difficulty, first.Stage);
        Assert.AreEqual(GameStageLabels.Gameplay, second.Stage);
        Assert.AreEqual(13, second.Tries);
    }

    [Test]
    public void Should_Return_Default_Progress_On_End_Response()
    {
        UdpClientStrategy strategy = makeTestable();
        _responses.Enqueue(ProtocolMessages.End());

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Return_Default_Progress_On_Other_Event()
    {
        UdpClientStrategy strategy = makeTestable();
        _responses.Enqueue(ProtocolMessages.OtherEvent(new() { Id = "42", Stage = GameStageLabels.Difficulty }));

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Start_New_Session_On_Setup_After_Game()
    {
        UdpClientStrategy strategy = makeTestable();
        _responses.Enqueue(ProtocolMessages.Progress(new() { Id = "42", Stage = GameStageLabels.Difficulty }));
        _responses.Enqueue(ProtocolMessages.Progress(new() { Id = "43", Stage = GameStageLabels.Difficulty }));

        Sync.Run(strategy.Setup);
        Sync.Run(() => strategy.StageAction(string.Empty));
        Sync.Run(strategy.Setup);
        Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        JObject[] requests = _requests.ToArray();
        Assert.True(requests[1]["data"]["initial"].Value<bool>(), message: "Request after setup should be initial again");
        Assert.False(requests[1].ContainsKey("id"), message: "Id should be dropped on setup");
    }

    [Test]
    public void Should_Read_Client_Port_From_Query()
    {
        UdpClientStrategy strategy = UdpClientStrategy.Of(new Uri($"udp://127.0.0.1:{_port}?client_port=0"));

        Assert.IsNotNull(strategy);
        strategy.Dispose();
    }

    private UdpClientStrategy makeTestable()
    {
        var client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return new UdpClientStrategy(client, "127.0.0.1", _port);
    }

    private async Task serve()
    {
        while (true)
        {
            UdpReceiveResult received;
            try { received = await _server.ReceiveAsync(); }
            catch (Exception) { return; }
            _requests.Enqueue(JObject.Parse(Encoding.UTF8.GetString(received.Buffer)));
            string response = _responses.TryDequeue(out string next) ? next : "{}";
            byte[] bytes = Encoding.UTF8.GetBytes(response);
            await _server.SendAsync(bytes, bytes.Length, received.RemoteEndPoint);
        }
    }
}
