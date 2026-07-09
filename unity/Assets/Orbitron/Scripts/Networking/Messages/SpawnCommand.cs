using System;

[Serializable]
public class SpawnCommand
{
    public string cmd  = "spawn";
    public string name;
    public double mass;
    public double rad;
    public float[] pos;
    public float[] vel;
}