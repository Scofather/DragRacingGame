using UnityEngine;
using UnityEngine.UI;

public class RaceTimer : MonoBehaviour
{
    public Text timerText;

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

        bool inputDetected = Input.GetMouseButton(0) || Input.GetAxis("Vertical") > 0f;
        if (!isRacing && inputDetected)
            isRacing = true;

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
        if (other.GetComponent<CarPhysics>() == null) return;

        isRacing = false;
        isFinished = true;
        if (timerText != null)
        {
            timerText.text = string.Format("Time: {0:F2}s", raceTime);
            timerText.color = Color.green;
        }
    }
}
