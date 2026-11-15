using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EngineAudio : MonoBehaviour
{
    public CarPhysics car;
    public float idleVolume     = 0.18f;
    public float throttleVolume = 0.5f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.clip   = BuildClip();
        source.loop   = true;
        source.volume = idleVolume;
        if (car != null) car.engineAudio = source; // CarPhysics drives the pitch from RPM
        source.Play();
    }

    private void Update()
    {
        if (car == null) return;
        float target = car.isAccelerating ? throttleVolume : idleVolume;
        source.volume = Mathf.MoveTowards(source.volume, target, 2f * Time.deltaTime);
    }

    // One-second loop; the fundamental is a whole number of cycles so the loop point is seamless.
    private static AudioClip BuildClip()
    {
        const int   rate     = 44100;
        const float baseFreq = 60f;
        var data = new float[rate];
        for (int i = 0; i < rate; i++)
        {
            float t = (float)i / rate;
            float s = 0f;
            for (int h = 1; h <= 8; h++)
                s += Mathf.Sin(2f * Mathf.PI * baseFreq * h * t) / h * (h % 2 == 0 ? 0.6f : 1f);
            data[i] = s * 0.35f;
        }
        var clip = AudioClip.Create("EngineLoop", rate, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
