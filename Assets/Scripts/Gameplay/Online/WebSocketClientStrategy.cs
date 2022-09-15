using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp;

public class WebSocketClientStrategy : IGameplayStrategy, IDisposable
{
    private readonly string _address;

    private WebSocket _socket;

    private TaskCompletionSource<ResponsePayload?> _messageData;

    public WebSocketClientStrategy(string address) => _address = address;

    public WebSocketClientStrategy(Uri address) => _address = address.AbsoluteUri;

    public async Task Setup()
    {
        Dispose();
        _socket = new(_address);
        TaskCompletionSource<bool> connection = new();
        var onOpen = new EventHandler((_, _) => connection.SetResult(true));
        _socket.OnOpen += onOpen;
        _socket.OnMessage += parseMessage;
        _socket.OnClose += clearMessageData;
        _messageData = new();
        _socket.ConnectAsync();
        await connection.Task;
        _socket.OnOpen -= onOpen;
    }

    public async Task<GameProgress> StageAction(string input)
    {
        if (_socket == null || _socket.ReadyState != WebSocketState.Open) return default;
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
        _socket.Close();
        _socket.OnMessage -= parseMessage;
        _socket.OnClose -= clearMessageData;
        _socket = null;
    }

    private void parseMessage(object _, MessageEventArgs e)
    {
        var response = JsonConvert.DeserializeObject<ResponsePayload?>(e.Data);
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

    private void clearMessageData(object _, CloseEventArgs e) => _messageData?.TrySetResult(null);
}
