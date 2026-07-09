using System;
using Newtonsoft.Json;

[Serializable]
public class SimulationState
{
    public int    step;

    [JsonProperty("sim_time")]
    public double simTime;

    public double energy;

    [JsonProperty("angular_momentum")]
    public double angularMomentum;

    [JsonProperty("merge_events")]
    public MergeEvent[] mergeEvents;

    public BodyState[] bodies;
}