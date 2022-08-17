using Newtonsoft.Json;

public struct GameProgress
{
    [JsonProperty(PropertyName="id", NullValueHandling=NullValueHandling.Ignore)]
    public string Id;

    [JsonProperty(PropertyName="difficulty", NullValueHandling=NullValueHandling.Ignore)]
    public string Difficulty;

    [JsonProperty(PropertyName="stage")]
    public string Stage;

    [JsonProperty(PropertyName="tries", NullValueHandling=NullValueHandling.Ignore)]
    public int Tries;

    [JsonProperty(PropertyName="guessed", NullValueHandling=NullValueHandling.Ignore)]
    public string[] Guessed;
}

