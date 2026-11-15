using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public enum WeatherType { Sunny, Rain, Snow }

    public WeatherType currentWeather = WeatherType.Sunny;
    public CarPhysics[] cars;

    [Header("Visuals")]
    public Light sun;
    public Camera cam;
    public ParticleSystem rain;
    public ParticleSystem snow;

    private WeatherType appliedWeather = (WeatherType)(-1);

    private void Update()
    {
        if (currentWeather == appliedWeather) return;
        appliedWeather = currentWeather;

        switch (currentWeather)
        {
            case WeatherType.Sunny:
                Apply(1.0f, new Color(1.0f, 0.93f, 0.78f), 1.3f, false, Color.black, 0f);
                break;
            case WeatherType.Rain:
                Apply(0.6f, new Color(0.55f, 0.60f, 0.70f), 0.6f, true, new Color(0.35f, 0.38f, 0.42f), 0.012f);
                break;
            case WeatherType.Snow:
                Apply(0.3f, new Color(0.85f, 0.88f, 0.95f), 0.9f, true, new Color(0.80f, 0.82f, 0.86f), 0.018f);
                break;
        }

        SetPlaying(rain, currentWeather == WeatherType.Rain);
        SetPlaying(snow, currentWeather == WeatherType.Snow);
    }

    private void Apply(float friction, Color sunColor, float sunIntensity, bool fog, Color fogColor, float fogDensity)
    {
        if (cars != null)
            foreach (var c in cars)
                if (c != null) c.frictionCoefficient = friction;

        if (sun != null)
        {
            sun.color     = sunColor;
            sun.intensity = sunIntensity;
        }

        RenderSettings.fog = fog;
        if (fog)
        {
            RenderSettings.fogMode    = FogMode.ExponentialSquared;
            RenderSettings.fogColor   = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }

        if (cam != null)
        {
            cam.clearFlags = fog ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            if (fog) cam.backgroundColor = fogColor;
        }
    }

    private static void SetPlaying(ParticleSystem ps, bool on)
    {
        if (ps == null) return;
        if (on) ps.Play();
        else    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
