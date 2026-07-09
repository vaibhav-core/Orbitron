using UnityEngine;

/// <summary>
/// Converts between Python simulation units (AU, solar masses, AU/yr, years)
/// and Unity world-space units.
///
/// Distance:  1 AU  = 100 Unity units  (linear)
/// Radius:    power-curve compressed — preserves relative ordering without
///            making small planets invisible or the Sun overwhelming the scene
/// Velocity:  AU/yr → Unity units/yr  (same linear factor as distance)
/// Time:      simulation years → human-readable string
/// Mass:      solar masses → Earth masses or solar masses for display
/// Speed:     AU/yr → km/s for stat panels judges will recognize
/// </summary>
public static class Normalizer
{
    // ── Scale factors ─────────────────────────────────────────────────────────

    public const float DistanceScale    = 100f;
    public const float VelocityScale    = DistanceScale;

    public const float RadiusExponent   = 0.45f;
    public const float RadiusVisualBase = 1.5f;
    public const float MinVisualRadius  = 0.3f;
    public const float MaxVisualRadius  = 15f;

    public const float EarthRadiusAU    = 4.2635e-5f;
    public const float EarthMassSolar   = 3.003e-6f;
    public const float AUperYearToKmS   = 4.7404f;

    // ── Position ──────────────────────────────────────────────────────────────

    public static Vector3 NormalizePosition(float x, float y)
    {
        return new Vector3(x * DistanceScale, 0f, y * DistanceScale);
    }

    public static (float x, float y) DenormalizePosition(Vector3 unityPos)
    {
        return (unityPos.x / DistanceScale, unityPos.z / DistanceScale);
    }

    // ── Radius ────────────────────────────────────────────────────────────────

    public static float NormalizeRadius(float radiusAU)
    {
        if (radiusAU <= 0f) return MinVisualRadius;
        float earthRelative = radiusAU / EarthRadiusAU;
        float visual = RadiusVisualBase * Mathf.Pow(earthRelative, RadiusExponent);
        return Mathf.Clamp(visual, MinVisualRadius, MaxVisualRadius);
    }

    public static float DenormalizeRadius(float visualDiameter)
    {
        float clamped = Mathf.Clamp(visualDiameter, MinVisualRadius, MaxVisualRadius);
        float earthRelative = Mathf.Pow(clamped / RadiusVisualBase, 1f / RadiusExponent);
        return earthRelative * EarthRadiusAU;
    }

    // ── Velocity ──────────────────────────────────────────────────────────────

    public static Vector3 NormalizeVelocity(float vx, float vy)
    {
        return new Vector3(vx * VelocityScale, 0f, vy * VelocityScale);
    }

    public static (float vx, float vy) DenormalizeVelocity(Vector3 unityVel)
    {
        return (unityVel.x / VelocityScale, unityVel.z / VelocityScale);
    }

    // ── Mass display ──────────────────────────────────────────────────────────

    public static double ToEarthMasses(double solarMasses)
    {
        return solarMasses / EarthMassSolar;
    }

    public static string FormatMass(double solarMasses)
    {
        double earthMasses = ToEarthMasses(solarMasses);

        if (earthMasses >= 10000.0)
            return $"{solarMasses:F4} M☉";
        else if (earthMasses >= 0.1)
            return $"{earthMasses:F1} M⊕";
        else
            return $"{solarMasses:E2} M☉";
    }

    // ── Speed display ─────────────────────────────────────────────────────────

    public static float ToKmPerSecond(float auPerYear)
    {
        return auPerYear * AUperYearToKmS;
    }

    public static string FormatSpeed(float vx, float vy)
    {
        float speedAUyr = Mathf.Sqrt(vx * vx + vy * vy);
        return $"{ToKmPerSecond(speedAUyr):F2} km/s";
    }

    // ── Simulated time display ────────────────────────────────────────────────

    public static string FormatSimTime(float simTimeYears)
    {
        float days = simTimeYears * 365.25f;

        if (days < 1f)
            return $"{days * 24f:F1} hours";
        else if (days < 60f)
            return $"{days:F1} days";
        else if (days < 730f)
            return $"{days / 30.44f:F1} months";
        else
            return $"{simTimeYears:F2} years";
    }

    // ── Acceleration display ──────────────────────────────────────────────────

    public static string FormatAcceleration(float ax, float ay)
    {
        float magAUyr2 = Mathf.Sqrt(ax * ax + ay * ay);
        float magMs2   = magAUyr2 * 1.502e-4f;

        if (magMs2 < 1e-6f)
            return $"{magMs2 * 1e9f:F3} nm/s²";
        else if (magMs2 < 1e-3f)
            return $"{magMs2 * 1e6f:F3} μm/s²";
        else
            return $"{magMs2:F6} m/s²";
    }

    // ── Distance from Sun display ─────────────────────────────────────────────

    public static string FormatDistanceAU(float x, float y)
    {
        float distAU = Mathf.Sqrt(x * x + y * y);
        return distAU < 0.01f
            ? $"{distAU * 1.496e8f:F0} km"
            : $"{distAU:F4} AU";
    }
}