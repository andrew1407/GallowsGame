using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

// Server-side messages of the gallows-remastered protocol, for the online client tests
public static class ProtocolMessages
{
    public static string Progress(GameProgress progress) => JsonConvert.SerializeObject(new ResponsePayload() {
        Event = RequestPayload.EVENT_NAME,
        Data = progress,
    });

    public static string End() => JsonConvert.SerializeObject(new ResponsePayload() {
        Event = RequestPayload.EVENT_NAME,
        End = true,
    });

    public static string OtherEvent(GameProgress progress) => JsonConvert.SerializeObject(new ResponsePayload() {
        Event = "sasik",
        Data = progress,
    });

    public static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
