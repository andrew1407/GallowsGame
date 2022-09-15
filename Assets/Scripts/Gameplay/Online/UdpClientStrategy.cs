using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class UdpClientStrategy : IGameplayStrategy, IDisposable
{
    private readonly string _host;

    private readonly int _port;

    private readonly UdpClient _client;

    private string _id;

    public static UdpClientStrategy Of(Uri uri)
    {
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        bool parsed = int.TryParse(query["client_port"], out int clientPort);
        int defaultPort = 8080;
        var client = new UdpClient(parsed ? clientPort : defaultPort);
        int port = uri.Port == -1 ? defaultPort : uri.Port;
        return new(client, uri.Host, port);
    }

    public UdpClientStrategy(UdpClient client, string host, int port)
    {
        _client = client;
        _host = host;
        _port = port;
    }
    
    public Task Setup()
    {
        if (string.IsNullOrEmpty(_id)) _client.Connect(_host, _port);
        _id = null;
        return Task.CompletedTask;
    }

    public async Task<GameProgress> StageAction(string input)
    {
        bool initial = string.IsNullOrEmpty(_id);
        await sendData(input, initial);
        var gameProgress = await acceptProgress();
        if (initial) _id = gameProgress.Id;
        return gameProgress;
    }

    public void Dispose()
    {
        _client?.Close();
        _client?.Dispose();
    }

    private async Task sendData(string input, bool initial)
    {
        var requestPayload = new RequestPayload() {
            Id = _id,
            Event = RequestPayload.EVENT_NAME,
            Data = new DataContainer() {
                Input = input,
                Initial = initial
            },
        };
        string stringified = JsonConvert.SerializeObject(requestPayload);
        byte[] bytes = Encoding.UTF8.GetBytes(stringified);
        await _client.SendAsync(bytes, bytes.Length);
    }

    private async Task<GameProgress> acceptProgress()
    {
        UdpReceiveResult received = await _client.ReceiveAsync();
        string receivedString = Encoding.UTF8.GetString(received.Buffer);
        var response = JsonConvert.DeserializeObject<ResponsePayload>(receivedString);
        bool invalid = response.End == true || response.Data == null || response.Event != RequestPayload.EVENT_NAME;
        return invalid ? default : response.Data.Value;
    }
}
