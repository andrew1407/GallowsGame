using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

// Sends a JSON body and returns the JSON response: HttpClientStrategy's only contact with the network
public interface IJsonPoster : IDisposable
{
    Task<string> Post(string address, string json);
}

public class HttpClientPoster : IJsonPoster
{
    private readonly HttpClient _client;

    public HttpClientPoster(HttpClient client) => _client = client;

    public async Task<string> Post(string address, string json)
    {
        var payload = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(address, payload);
        return await response.Content.ReadAsStringAsync();
    }

    public void Dispose() => _client.Dispose();
}

// Browsers have no sockets for HttpClient, but UnityWebRequest runs on the browser's own fetch
// (the server has to allow the page's origin with CORS)
public class WebRequestPoster : IJsonPoster
{
    public async Task<string> Post(string address, string json)
    {
        using var request = new UnityWebRequest(address, UnityWebRequest.kHttpVerbPOST) {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer(),
        };
        request.SetRequestHeader("Content-Type", "application/json");
        TaskCompletionSource<bool> sent = new();
        request.SendWebRequest().completed += _ => sent.TrySetResult(true);
        await sent.Task;
        if (request.result != UnityWebRequest.Result.Success)
            throw new HttpRequestException($"POST {address} failed: {request.error}");
        return request.downloadHandler.text;
    }

    public void Dispose() { }
}
