using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

public class HttpClientStrategy : IGameplayStrategy, IDisposable
{
    private readonly HttpClient _client;

    private readonly string _address;

    private string _id;

    public HttpClientStrategy(HttpClient client, Uri address)
    {
        _client = client;
        string uri = address.AbsoluteUri;
        uri += uri.EndsWith('/') ? "nextStage" : "/nextStage";
        _address = uri;
    }

    public Task Setup()
    {
        _id = null;
        return Task.CompletedTask;
    }

    public async Task<GameProgress> StageAction(string input)
    {
        bool initial = string.IsNullOrEmpty(_id);
        var response = await (initial ? request(initital: true) : request(_id, input));
        if (response.End == true || response.Data == null) return default;
        var gameProgress = response.Data.Value;
        if (initial) _id = gameProgress.Id;
        return gameProgress;
    }

    public void Dispose() => _client.Dispose();

    private async Task<ResponsePayload> request(string id = null, string input = null, bool? initital = null)
    {
        var payloadData = new RequestPayload() {
            Id = id,
            Data = new DataContainer() {
                Initial = initital,
                Input = input,
            },
        };
        string stringified = JsonConvert.SerializeObject(payloadData);
        var payload = new StringContent(stringified, System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_address, payload);
        string json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResponsePayload>(json);
    }
}
