using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using System.IO;
using Newtonsoft.Json;

public class TcpClientStrategy : IGameplayStrategy, IDisposable
{
    private readonly string _host;

    private readonly int _port;

    private TcpClient _client = new();

    private string _id;

    public TcpClientStrategy(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public TcpClientStrategy(Uri uri)
    {
        _host = uri.Host;
        int defaultPort = 8080;
        _port = uri.Port == -1 ? defaultPort : uri.Port;
    }

    public async Task Setup()
    {
        Dispose();
        _client = new();
        if (string.IsNullOrEmpty(_id))
        {
            await _client.ConnectAsync(_host, _port);
            await acceptProgress();
        }
        _id = null;
    }

    public async Task<GameProgress> StageAction(string input)
    {
        if (!_client.Connected) return default;
        bool initial = string.IsNullOrEmpty(_id);
        await sendData(input, initial);
        var gameProgress = await acceptProgress();
        if (initial) _id = gameProgress.Id;
        return gameProgress;
    }

    public void Dispose()
    {
        if (_client.Connected) _client.Close();
        _client.Dispose();
    }

    private async Task sendData(string input, bool initial)
    {
        if (!_client.Connected) return;
        var requestPayload = new RequestPayload()
        {
            Id = _id,
            Event = RequestPayload.EVENT_NAME,
            Data = new DataContainer()
            {
                Input = input,
                Initial = initial
            },
        };
        string stringified = JsonConvert.SerializeObject(requestPayload);
        byte[] bytes = Encoding.UTF8.GetBytes(stringified);
        NetworkStream stream = _client.GetStream();
        await stream.WriteAsync(bytes);
    }

    private async Task<GameProgress> acceptProgress()
    {
        if (!_client.Connected) return default;
        var buffer = new byte[1024];
        NetworkStream stream = _client.GetStream();

        try
        {
            await stream.ReadAsync(buffer);
            string receivedString = Encoding.UTF8.GetString(buffer);
            var response = JsonConvert.DeserializeObject<ResponsePayload>(receivedString);
            bool invalid = response.End == true || response.Data == null || response.Event != RequestPayload.EVENT_NAME;
            return invalid ? default : response.Data.Value;
        }
        catch (IOException)
        {
            return default;
        }
    }
}
