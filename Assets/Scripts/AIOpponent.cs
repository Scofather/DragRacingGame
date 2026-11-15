using UnityEngine;

public class AIOpponent : MonoBehaviour
{
    public CarPhysics car;
    public RaceTimer  raceTimer;

    [Header("Difficulty")]
    public float reactionDelay  = 0.35f;
    public float speedBias      = 0.97f;  // <1 = easier, >1 = harder
    public float randomVariance = 0.05f;  // random speed nudge each race

    [Header("Finish")]
    public float finishZ          = 402.33f;
    public float stopDeceleration = 40f;

    public bool  HasFinished => hasFinished;
    public float FinishTime  => finishTime;

    private bool  raceStarted;
    private bool  accelerating;
    private bool  hasFinished;
    private float reactionTimer;
    private float elapsed;
    private float finishTime;

    private bool  baseCaptured;
    private float baseMaxVelocity;
    private float baseMaxTorque;

    private void Start()
    {
        if (!baseCaptured) Configure(speedBias, reactionDelay);
    }

    public void Configure(float bias, float reaction)
    {
        if (car == null) return;
        if (!baseCaptured)
        {
            baseCaptured    = true;
            baseMaxVelocity = car.maxVelocity;
            baseMaxTorque   = car.maxTorque;
        }

        speedBias = bias;
        float b = Mathf.Clamp(bias + Random.Range(-randomVariance, randomVariance), 0.7f, 1.2f);
        car.maxVelocity = baseMaxVelocity * b;
        car.maxTorque   = baseMaxTorque   * b;
        reactionDelay   = Mathf.Max(0f, reaction + Random.Range(-0.12f, 0.12f));
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
                elapsed       = raceTimer.FinalTime;
            }
            return;
        }

        if (hasFinished)
        {
            car.ApplyThrottle(0f);
            car.velocity = Mathf.Max(0f, car.velocity - stopDeceleration * Time.deltaTime);
            return;
        }

        // Own clock: the race timer stops when the player finishes, the rival may still be running.
        elapsed += Time.deltaTime;

        if (!accelerating)
        {
            reactionTimer -= Time.deltaTime;
            if (reactionTimer <= 0f) accelerating = true;
        }

        if (accelerating) car.ApplyThrottle(1f);

        if (transform.position.z >= finishZ)
        {
            hasFinished = true;
            finishTime  = elapsed;
        }
    }
}
