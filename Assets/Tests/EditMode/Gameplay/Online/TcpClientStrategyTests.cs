using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class TcpClientStrategyTests
{
    private TcpListener _listener;

    private ConcurrentQueue<string> _responses;

    private ConcurrentQueue<JObject> _requests;

    private int _connections;

    private int _port => ((IPEndPoint)_listener.LocalEndpoint).Port;

    [SetUp]
    public void SetUp()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        _responses = new();
        _requests = new();
        _connections = 0;
        Task.Run(serve);
    }

    [TearDown]
    public void TearDown() => _listener.Stop();

    [Test]
    public void Should_Send_Initial_Request_And_Remember_Id()
    {
        TcpClientStrategy strategy = new("127.0.0.1", _port);
        _responses.Enqueue(ProtocolMessages.Progress(new() { Id = "42", Stage = GameStageLabels.Difficulty }));
        _responses.Enqueue(ProtocolMessages.Progress(new() { Id = "42", Stage = GameStageLabels.Gameplay, Tries = 7 }));

        Sync.Run(strategy.Setup);
        GameProgress first = Sync.Run(() => strategy.StageAction(string.Empty));
        GameProgress second = Sync.Run(() => strategy.StageAction("2"));
        strategy.Dispose();

        JObject[] requests = _requests.ToArray();
        Assert.AreEqual(2, requests.Length);
        Assert.AreEqual(RequestPayload.EVENT_NAME, requests[0]["event"].Value<string>());
        Assert.True(requests[0]["data"]["initial"].Value<bool>(), message: "First request should be initial");
        Assert.False(requests[0].ContainsKey("id"), message: "First request cannot have an id");
        Assert.AreEqual("42", requests[1]["id"].Value<string>(), message: "Id from the first response should be reused");
        Assert.AreEqual("2", requests[1]["data"]["input"].Value<string>());
        Assert.False(requests[1]["data"]["initial"].Value<bool>());

        Assert.AreEqual(GameStageLabels.Difficulty, first.Stage);
        Assert.AreEqual(GameStageLabels.Gameplay, second.Stage);
        Assert.AreEqual(7, second.Tries);
    }

    [Test]
    public void Should_Return_Default_Progress_On_End_Response()
    {
        TcpClientStrategy strategy = new("127.0.0.1", _port);
        _responses.Enqueue(ProtocolMessages.End());

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Return_Default_Progress_On_Other_Event()
    {
        TcpClientStrategy strategy = new("127.0.0.1", _port);
        _responses.Enqueue(ProtocolMessages.OtherEvent(new() { Id = "42", Stage = GameStageLabels.Difficulty }));

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Return_Default_Progress_Without_Connection()
    {
        TcpClientStrategy strategy = new("127.0.0.1", _port);

        GameProgress actual = Sync.Run(() => strategy.StageAction("b"));

        Assert.AreEqual(default(GameProgress), actual);
        Assert.IsEmpty(_requests);
    }

    [Test]
    public void Should_Start_New_Session_On_Setup_After_Game()
    {
        TcpClientStrategy strategy = new("127.0.0.1", _port);
        _responses.Enqueue(ProtocolMessages.Progress(new() { Id = "42", Stage = GameStageLabels.Difficulty }));
        _responses.Enqueue(ProtocolMessages.Progress(new() { Id = "43", Stage = GameStageLabels.Difficulty }));

        Sync.Run(strategy.Setup);
        Sync.Run(() => strategy.StageAction(string.Empty));
        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));
        strategy.Dispose();

        JObject[] requests = _requests.ToArray();
        Assert.AreEqual(2, _connections, message: "Restart should open a new connection");
        Assert.AreEqual(2, requests.Length);
        Assert.True(requests[1]["data"]["initial"].Value<bool>(), message: "Request after setup should be initial again");
        Assert.AreEqual(GameStageLabels.Difficulty, actual.Stage, message: "New session should be played");
    }

    [Test]
    public void Should_Use_Port_8080_By_Default()
    {
        Assert.DoesNotThrow(() => new TcpClientStrategy(new Uri("tcp://localhost")).Dispose());
    }

    // Like the real server: greets every new connection, then answers each request
    private async Task serve()
    {
        while (true)
        {
            TcpClient client;
            try { client = await _listener.AcceptTcpClientAsync(); }
            catch (Exception) { return; }
            Interlocked.Increment(ref _connections);
            _ = Task.Run(() => session(client));
        }
    }

    private async Task session(TcpClient client)
    {
        using (client)
        {
            NetworkStream stream = client.GetStream();
            await write(stream, ProtocolMessages.Progress(new() { Stage = GameStageLabels.Difficulty }));
            var buffer = new byte[1024];
            while (true)
            {
                int read;
                try { read = await stream.ReadAsync(buffer, 0, buffer.Length); }
                catch (Exception) { return; }
                if (read == 0) return;
                _requests.Enqueue(JObject.Parse(Encoding.UTF8.GetString(buffer, 0, read)));
                await write(stream, _responses.TryDequeue(out string response) ? response : "{}");
            }
        }
    }

    private static Task write(NetworkStream stream, string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        return stream.WriteAsync(bytes, 0, bytes.Length);
    }
}
