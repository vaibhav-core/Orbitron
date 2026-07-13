using System;

[Serializable]
public class BodyState
{
    public string name;
    public string parent;
    public string type;
    public string status;

    public double[] pos;
    public double[] vel;
    public double[] acc;

    public double mass;
    public double rad;

    public double escape_velocity;
    public double orbital_velocity;
    public double total_energy;
    public double orbital_period;
}