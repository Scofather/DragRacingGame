using UnityEngine;

public class CarPhysics : MonoBehaviour
{
    [Header("Engine")]
    public float maxTorque = 2000f;
    public float[] gearRatios = { 3.5f, 2.5f, 1.8f, 1.3f, 1.0f, 0.8f };
    public float currentRPM = 0f;
    public int currentGear = 1;
    public float maxRPM = 8000f;

    [Header("Physics")]
    public float mass = 1500f;
    public float frictionCoefficient = 1.0f;
    public float dragCoefficient = 0.5f;
    public float maxVelocity = 100f;
    public float wheelspinSpeedLimit = 15f;

    [Header("State")]
    public float velocity = 0f;
    public bool isAccelerating = false;
    public bool isManual = false;

    [Header("Audio & VFX")]
    public AudioSource engineAudio;
    public float minPitch = 0.8f;
    public float maxPitch = 2.5f;
    public ParticleSystem[] tireSmokeParticles;

    private const float WheelRadius = 0.3f;

    public void ApplyThrottle(float input)
    {
        isAccelerating = input > 0f;

        float engineForce = ((maxTorque * input * frictionCoefficient) * gearRatios[currentGear - 1]) / WheelRadius;
        float dragForce = dragCoefficient * (velocity * velocity);

        velocity += ((engineForce - dragForce) / mass) * Time.deltaTime;
        velocity = Mathf.Clamp(velocity, 0f, maxVelocity);

        currentRPM = velocity * gearRatios[currentGear - 1] * 100f;

        if (!isManual && currentRPM > maxRPM && currentGear < gearRatios.Length)
            currentGear++;

        transform.position += transform.forward * velocity * Time.deltaTime;
        UpdateEffects();
    }

    public void ShiftUp()
    {
        if (currentGear < gearRatios.Length)
            currentGear++;
    }

    public void ShiftDown()
    {
        if (currentGear > 1)
            currentGear--;
    }

    private void UpdateEffects()
    {
        bool smokingWheels = isAccelerating && velocity < wheelspinSpeedLimit;
        if (tireSmokeParticles == null) return;
        foreach (var ps in tireSmokeParticles)
        {
            if (ps == null) continue;
            var emission = ps.emission;
            emission.enabled = smokingWheels;
            if (smokingWheels && !ps.isPlaying) ps.Play();
            else if (!smokingWheels && ps.isPlaying) ps.Stop();
        }
    }

    private void Update()
    {
        if (engineAudio != null)
            engineAudio.pitch = Mathf.Lerp(minPitch, maxPitch, currentRPM / maxRPM);
    }
}
