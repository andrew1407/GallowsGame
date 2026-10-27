using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class PayloadSerializationTests
{
    [Test]
    public void Should_Omit_Null_Request_Fields()
    {
        RequestPayload payload = new() { Data = new DataContainer() { Initial = true } };

        JObject json = JObject.Parse(JsonConvert.SerializeObject(payload));

        Assert.False(json.ContainsKey("id"), message: "Null id should be omitted");
        Assert.False(json.ContainsKey("event"), message: "Null event should be omitted");
        Assert.False(((JObject)json["data"]).ContainsKey("input"), message: "Null input should be omitted");
        Assert.AreEqual(true, json["data"]["initial"].Value<bool>());
    }

    [Test]
    public void Should_Serialize_Request_With_Lowercase_Keys()
    {
        RequestPayload payload = new() {
            Id = "42",
            Event = RequestPayload.EVENT_NAME,
            Data = new DataContainer() { Input = "b" },
        };

        JObject json = JObject.Parse(JsonConvert.SerializeObject(payload));

        Assert.AreEqual("42", json["id"].Value<string>());
        Assert.AreEqual("nextStage", json["event"].Value<string>());
        Assert.AreEqual("b", json["data"]["input"].Value<string>());
        Assert.False(json.ContainsKey("EVENT_NAME"), message: "Constants shouldn't be serialized");
    }

    [Test]
    public void Should_Deserialize_Response_With_Game_Progress()
    {
        string json = @"{
            ""event"": ""nextStage"",
            ""data"": { ""id"": ""42"", ""difficulty"": ""NORMAL"", ""stage"": ""gameplay"", ""tries"": 7, ""guessed"": [""b"", null, ""b"", null] }
        }";

        var response = JsonConvert.DeserializeObject<ResponsePayload>(json);

        Assert.IsNull(response.End);
        Assert.AreEqual(RequestPayload.EVENT_NAME, response.Event);
        Assert.True(response.Data.HasValue, message: "Response data should be parsed");
        GameProgress progress = response.Data.Value;
        Assert.AreEqual("42", progress.Id);
        Assert.AreEqual("NORMAL", progress.Difficulty);
        Assert.AreEqual(GameStageLabels.Gameplay, progress.Stage);
        Assert.AreEqual(7, progress.Tries);
        Assert.AreEqual(new string[] { "b", null, "b", null }, progress.Guessed);
    }

    [Test]
    public void Should_Deserialize_End_Response_Without_Data()
    {
        var response = JsonConvert.DeserializeObject<ResponsePayload>(@"{ ""end"": true }");

        Assert.AreEqual(true, response.End);
        Assert.False(response.Data.HasValue);
    }

    [Test]
    public void Should_Keep_Null_Stage_In_Serialized_Progress()
    {
        GameProgress progress = new() { Tries = 3 };

        JObject json = JObject.Parse(JsonConvert.SerializeObject(progress));

        Assert.True(json.ContainsKey("stage"), message: "Stage key should always be present");
        Assert.AreEqual(JTokenType.Null, json["stage"].Type);
        Assert.False(json.ContainsKey("id"));
        Assert.False(json.ContainsKey("guessed"));
    }
}
