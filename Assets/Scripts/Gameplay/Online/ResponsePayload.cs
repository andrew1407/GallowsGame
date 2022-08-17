using Newtonsoft.Json;

public struct ResponsePayload
{
    [JsonProperty(PropertyName="end")]
    public bool? End;

    [JsonProperty(PropertyName="data")]
    public GameProgress? Data;

    [JsonProperty(PropertyName="event")]
    public string Event;
}
