using System;

[Serializable]
public class BodyState
{
    public string name;

    public double[] pos;
    public double[] vel;
    public double[] acc;

    public double mass;
    public double rad;
}