using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WebSocketSharp;

// A text WebSocket as WebSocketClientStrategy sees it: desktop players run on WebSocketSharp, the web player on the browser's own WebSocket
public interface IWebSocketConnection : IDisposable
{
    event Action Opened;

    event Action<string> MessageReceived;

    // Also raised when the connection fails before opening
    event Action Closed;

    bool IsOpen { get; }

    void Connect();

    void Send(string message);
}

public static class WebSocketConnection
{
    public static IWebSocketConnection Create(string address)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return new BrowserWebSocketConnection(address);
#else
        return new SharpWebSocketConnection(address);
#endif
    }
}

public class SharpWebSocketConnection : IWebSocketConnection
{
    private readonly WebSocket _socket;

    public event Action Opened;

    public event Action<string> MessageReceived;

    public event Action Closed;

    public bool IsOpen => _socket.ReadyState == WebSocketState.Open;

    public SharpWebSocketConnection(string address)
    {
        _socket = new WebSocket(address);
        _socket.OnOpen += (_, _) => Opened?.Invoke();
        _socket.OnMessage += (_, e) => MessageReceived?.Invoke(e.Data);
        _socket.OnClose += (_, _) => Closed?.Invoke();
    }

    public void Connect() => _socket.ConnectAsync();

    public void Send(string message) => _socket.Send(message);

    public void Dispose() => _socket.Close();
}

#if UNITY_WEBGL && !UNITY_EDITOR
// Bridges to Plugins/WebGL/BrowserWebSocket.jslib; the browser calls back by connection id
public class BrowserWebSocketConnection : IWebSocketConnection
{
    private delegate void SocketCallback(int id);

    private delegate void MessageCallback(int id, IntPtr message);

    [DllImport("__Internal")]
    private static extern int BrowserWebSocket_Connect(string address, SocketCallback onOpen, MessageCallback onMessage, SocketCallback onClose);

    [DllImport("__Internal")]
    private static extern void BrowserWebSocket_Send(int id, string message);

    [DllImport("__Internal")]
    private static extern void BrowserWebSocket_Close(int id);

    private static readonly Dictionary<int, BrowserWebSocketConnection> _connections = new();

    private readonly string _address;

    private int _id;

    public event Action Opened;

    public event Action<string> MessageReceived;

    public event Action Closed;

    public bool IsOpen { get; private set; }

    public BrowserWebSocketConnection(string address) => _address = address;

    public void Connect()
    {
        _id = BrowserWebSocket_Connect(_address, onOpen, onMessage, onClose);
        _connections[_id] = this;
    }

    public void Send(string message) => BrowserWebSocket_Send(_id, message);

    public void Dispose()
    {
        if (_id != 0) BrowserWebSocket_Close(_id);
    }

    [AOT.MonoPInvokeCallback(typeof(SocketCallback))]
    private static void onOpen(int id)
    {
        if (!_connections.TryGetValue(id, out var connection)) return;
        connection.IsOpen = true;
        connection.Opened?.Invoke();
    }

    [AOT.MonoPInvokeCallback(typeof(MessageCallback))]
    private static void onMessage(int id, IntPtr message)
    {
        if (_connections.TryGetValue(id, out var connection))
            connection.MessageReceived?.Invoke(Marshal.PtrToStringUTF8(message));
    }

    [AOT.MonoPInvokeCallback(typeof(SocketCallback))]
    private static void onClose(int id)
    {
        if (!_connections.Remove(id, out var connection)) return;
        connection.IsOpen = false;
        connection.Closed?.Invoke();
    }
}
#endif
