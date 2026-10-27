using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class HttpClientStrategyTests
{
    [Test]
    public void Should_Post_To_Next_Stage_Endpoint()
    {
        string[] addresses = { "http://localhost:8080", "http://localhost:8080/", "http://localhost:8080/api/" };
        string[] expected = { "http://localhost:8080/nextStage", "http://localhost:8080/nextStage", "http://localhost:8080/api/nextStage" };

        for (int i = 0; i < addresses.Length; ++i)
        {
            HandlerMock handler = new();
            HttpClientStrategy strategy = makeTestable(handler, addresses[i]);

            Sync.Run(strategy.Setup);
            Sync.Run(() => strategy.StageAction(string.Empty));

            Assert.AreEqual(HttpMethod.Post, handler.Requests[0].Method);
            Assert.AreEqual(expected[i], handler.Requests[0].Uri, message: $"Invalid endpoint for {addresses[i]}");
        }
    }

    [Test]
    public void Should_Send_Initial_Request_First_And_Remember_Id()
    {
        HandlerMock handler = new();
        HttpClientStrategy strategy = makeTestable(handler);
        handler.Responses.Enqueue(progressResponse(new() { Id = "42", Stage = GameStageLabels.Difficulty }));
        handler.Responses.Enqueue(progressResponse(new() { Id = "42", Stage = GameStageLabels.Gameplay }));

        Sync.Run(strategy.Setup);
        GameProgress first = Sync.Run(() => strategy.StageAction(string.Empty));
        GameProgress second = Sync.Run(() => strategy.StageAction("2"));

        JObject initialBody = handler.Requests[0].Body;
        Assert.True(initialBody["data"]["initial"].Value<bool>(), message: "First request should be initial");
        Assert.False(initialBody.ContainsKey("id"), message: "First request cannot have an id");

        JObject nextBody = handler.Requests[1].Body;
        Assert.AreEqual("42", nextBody["id"].Value<string>(), message: "Id from the first response should be reused");
        Assert.AreEqual("2", nextBody["data"]["input"].Value<string>());
        Assert.False(((JObject)nextBody["data"]).ContainsKey("initial"), message: "Next requests shouldn't be initial");

        Assert.AreEqual(GameStageLabels.Difficulty, first.Stage);
        Assert.AreEqual(GameStageLabels.Gameplay, second.Stage);
    }

    [Test]
    public void Should_Return_Default_Progress_On_End_Response()
    {
        HandlerMock handler = new();
        HttpClientStrategy strategy = makeTestable(handler);
        handler.Responses.Enqueue(@"{ ""end"": true, ""data"": { ""stage"": ""win"" } }");

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Return_Default_Progress_On_Empty_Data()
    {
        HandlerMock handler = new();
        HttpClientStrategy strategy = makeTestable(handler);
        handler.Responses.Enqueue("{}");

        Sync.Run(strategy.Setup);
        GameProgress actual = Sync.Run(() => strategy.StageAction(string.Empty));

        Assert.AreEqual(default(GameProgress), actual);
    }

    [Test]
    public void Should_Start_New_Session_After_Setup()
    {
        HandlerMock handler = new();
        HttpClientStrategy strategy = makeTestable(handler);
        handler.Responses.Enqueue(progressResponse(new() { Id = "42", Stage = GameStageLabels.Difficulty }));

        Sync.Run(strategy.Setup);
        Sync.Run(() => strategy.StageAction(string.Empty));
        Sync.Run(strategy.Setup);
        Sync.Run(() => strategy.StageAction(string.Empty));

        JObject body = handler.Requests[1].Body;
        Assert.True(body["data"]["initial"].Value<bool>(), message: "Request after setup should be initial again");
        Assert.False(body.ContainsKey("id"), message: "Id should be dropped on setup");
    }

    [Test]
    public void Should_Dispose_Http_Client()
    {
        HandlerMock handler = new();
        HttpClientStrategy strategy = makeTestable(handler);

        strategy.Dispose();

        Assert.True(handler.Disposed, message: "Http client (and its handler) should be disposed");
    }

    private HttpClientStrategy makeTestable(HandlerMock handler, string address = "http://localhost:8080")
    {
        return new HttpClientStrategy(new HttpClient(handler), new Uri(address));
    }

    private static string progressResponse(GameProgress progress)
    {
        return JsonConvert.SerializeObject(new ResponsePayload() { Event = RequestPayload.EVENT_NAME, Data = progress });
    }

    private class HandlerMock : HttpMessageHandler
    {
        public readonly List<(HttpMethod Method, string Uri, JObject Body)> Requests = new();

        public readonly Queue<string> Responses = new();

        public bool Disposed { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string body = await request.Content.ReadAsStringAsync();
            Requests.Add((request.Method, request.RequestUri.AbsoluteUri, JObject.Parse(body)));
            string response = Responses.Count > 0 ? Responses.Dequeue() : "{}";
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            };
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
