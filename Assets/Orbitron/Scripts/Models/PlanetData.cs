using System;
using UnityEngine;

public enum CelestialBodyType
{
    Star,
    Planet,
    Moon,
    Asteroid
}

[Serializable]
public class PlanetData
{
    public string           bodyName;
    public double           mass;
    public double           radius;
    public Vector3          initialPosition;
    public Vector3          initialVelocity;
    public CelestialBodyType bodyType;
    public string           textureKey;

    [HideInInspector] public string uniqueID;

    public PlanetData()
    {
        uniqueID = Guid.NewGuid().ToString();
    }
}