using UnityEngine;
using UnityEngine.UI;

public class RaceTimer : MonoBehaviour
{
    public Text timerText;
    [Tooltip("The player's car. Only this car stops the clock; leave empty to accept any non-AI car.")]
    public CarPhysics playerCar;

    private float raceTime = 0f;
    private bool isRacing = false;
    private bool isFinished = false;

    public bool IsFinished => isFinished;
    public bool IsRacing   => isRacing;
    public float FinalTime  => raceTime;

    public void StartRace()
    {
        if (!isFinished) isRacing = true;
    }

    private void Update()
    {
        if (isFinished) return;

        if (isRacing)
        {
            raceTime += Time.deltaTime;
            if (timerText != null)
                timerText.text = string.Format("Time: {0:F2}s", raceTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFinished) return;
        CarPhysics crossing = other.GetComponentInParent<CarPhysics>();
        if (crossing == null) return;
        // The rival's trigger collider also fires OnTriggerEnter; it must not end the player's race.
        if (playerCar != null ? crossing != playerCar : crossing.GetComponent<AIOpponent>() != null) return;

        isRacing = false;
        isFinished = true;
        if (timerText != null)
        {
            timerText.text = string.Format("Time: {0:F2}s", raceTime);
            timerText.color = Color.green;
        }
    }
}
