using System;
using Newtonsoft.Json;

[Serializable]
public class EditCommand
{
    public string cmd = "edit";
    public string name;

    // NullValueHandling.Ignore means these fields are omitted from the JSON
    // entirely when null, so Python never sees "mass": null and tries float(None)
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public double? mass;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public double? rad;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float[] pos;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float[] vel;
}