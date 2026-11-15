using UnityEngine;

public class AIOpponent : MonoBehaviour
{
    public CarPhysics car;
    public RaceTimer  raceTimer;

    [Header("Difficulty")]
    public float reactionDelay  = 0.35f;
    public float speedBias      = 0.97f;  // <1 = easier, >1 = harder
    public float randomVariance = 0.05f;  // random speed nudge each race

    private bool  raceStarted;
    private bool  accelerating;
    private float reactionTimer;

    private void Awake()
    {
        reactionDelay = Mathf.Max(0f, reactionDelay + Random.Range(-0.12f, 0.12f));
    }

    private void Start()
    {
        if (car == null) return;
        float bias = speedBias + Random.Range(-randomVariance, randomVariance);
        bias = Mathf.Clamp(bias, 0.7f, 1.2f);
        car.maxVelocity *= bias;
        car.maxTorque   *= bias;
    }

    private void Update()
    {
        if (raceTimer == null || car == null) return;

        if (!raceStarted)
        {
            if (raceTimer.IsRacing)
            {
                raceStarted   = true;
                reactionTimer = reactionDelay;
            }
            return;
        }

        if (!accelerating)
        {
            reactionTimer -= Time.deltaTime;
            if (reactionTimer <= 0f) accelerating = true;
        }

        if (accelerating && !raceTimer.IsFinished)
            car.ApplyThrottle(1f);
    }
}
