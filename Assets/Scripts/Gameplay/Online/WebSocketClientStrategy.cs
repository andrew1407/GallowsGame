using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class WebSocketClientStrategy : IGameplayStrategy, IDisposable
{
    private readonly string _address;

    private readonly Func<string, IWebSocketConnection> _connect;

    private IWebSocketConnection _socket;

    private TaskCompletionSource<ResponsePayload?> _messageData;

    public WebSocketClientStrategy(string address, Func<string, IWebSocketConnection> connect = null)
    {
        _address = address;
        _connect = connect ?? WebSocketConnection.Create;
    }

    public WebSocketClientStrategy(Uri address) : this(address.AbsoluteUri) { }

    public async Task Setup()
    {
        Dispose();
        IWebSocketConnection socket = _connect(_address);
        _socket = socket;
        TaskCompletionSource<bool> connection = new();
        Action onOpen = () => connection.TrySetResult(true);
        // Without this an unreachable server would keep the game waiting forever
        Action onFailed = () => connection.TrySetException(new IOException($"Can't connect to {_address}"));
        socket.Opened += onOpen;
        socket.Closed += onFailed;
        socket.MessageReceived += parseMessage;
        socket.Closed += clearMessageData;
        _messageData = new();
        socket.Connect();
        try
        {
            await connection.Task;
        }
        finally
        {
            socket.Opened -= onOpen;
            socket.Closed -= onFailed;
        }
    }

    public async Task<GameProgress> StageAction(string input)
    {
        if (_socket == null || !_socket.IsOpen) return default;
        if (_messageData == null) sendInput(input);
        var response = await _messageData.Task;
        _messageData = null;
        if (!response.HasValue) return default;
        var entries = response.Value;
        bool invalid = !entries.Data.HasValue || entries.End == true || entries.Event != RequestPayload.EVENT_NAME;
        return invalid ? default : entries.Data.Value;
    }

    public void Dispose()
    {
        if (_socket == null) return;
        _socket.MessageReceived -= parseMessage;
        _socket.Closed -= clearMessageData;
        _socket.Dispose();
        _socket = null;
    }

    private void parseMessage(string message)
    {
        var response = JsonConvert.DeserializeObject<ResponsePayload?>(message);
        _messageData?.TrySetResult(response);
    }

    private void sendInput(string input)
    {
        var requestPayload = new RequestPayload() {
            Event = RequestPayload.EVENT_NAME,
            Data = new DataContainer() { Input = input },
        };
        _messageData = new();
        _socket.Send(JsonConvert.SerializeObject(requestPayload));
    }

    private void clearMessageData() => _messageData?.TrySetResult(null);
}
