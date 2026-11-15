using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public enum WeatherType { Sunny, Rain, Snow }

    public WeatherType currentWeather = WeatherType.Sunny;
    public CarPhysics car;

    private void Update()
    {
        if (car == null) return;

        switch (currentWeather)
        {
            case WeatherType.Sunny: car.frictionCoefficient = 1.0f; break;
            case WeatherType.Rain:  car.frictionCoefficient = 0.6f; break;
            case WeatherType.Snow:  car.frictionCoefficient = 0.3f; break;
        }
    }
}
