using Newtonsoft.Json;

public struct DataContainer
{
    [JsonProperty(PropertyName="input", NullValueHandling=NullValueHandling.Ignore)]
    public string Input;

    [JsonProperty(PropertyName="initial", NullValueHandling=NullValueHandling.Ignore)]
    public bool? Initial;
}

public struct RequestPayload
{
    [JsonIgnore]
    public const string EVENT_NAME = "nextStage";
    
    [JsonProperty(PropertyName="id", NullValueHandling=NullValueHandling.Ignore)]
    public string Id;

    [JsonProperty(PropertyName="event", NullValueHandling=NullValueHandling.Ignore)]
    public string Event;

    [JsonProperty(PropertyName="data")]
    public DataContainer Data;
}
