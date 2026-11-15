using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public CarPhysics car;
    public Text speedText;
    public Text gearText;
    public Slider rpmBar;
    public RaceTimer raceTimer;

    [Header("Countdown")]
    public Text countdownText;

    [Header("Post-Race")]
    public GameObject finishPanel;
    public Text finishTimeText;
    public Text finishSpeedText;

    [Header("Nitro")]
    public NitroBoost nitroBoost;
    public Slider nitroBar;

    [Header("Auto Race (demo)")]
    public bool autoRace = false;
    public float autoRaceDelay = 3f;
    private float autoRaceTimer = 0f;
    private bool autoRaceLaunched = false;
    private float goTimer = 0f;

    private bool isHoldingGas = false;
    private float maxSpeed = 0f;
    private bool finishShown = false;

    public void OnGasPedalDown() => isHoldingGas = true;
    public void OnGasPedalUp()   => isHoldingGas = false;

    private void Update()
    {
        if (raceTimer != null && raceTimer.IsFinished)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        float throttle = 0f;

        if (autoRace)
        {
            if (!autoRaceLaunched)
            {
                autoRaceTimer += Time.deltaTime;

                if (countdownText != null)
                {
                    countdownText.gameObject.SetActive(true);
                    float r = autoRaceDelay - autoRaceTimer;
                    if      (r > 2f) { countdownText.text = "3"; countdownText.color = Color.white; }
                    else if (r > 1f) { countdownText.text = "2"; countdownText.color = Color.white; }
                    else             { countdownText.text = "1"; countdownText.color = new Color(1f, 0.35f, 0.35f); }
                }

                if (autoRaceTimer >= autoRaceDelay)
                {
                    autoRaceLaunched = true;
                    if (raceTimer != null) raceTimer.StartRace();
                    if (countdownText != null)
                    {
                        countdownText.text  = "GO!";
                        countdownText.color = new Color(0.15f, 0.9f, 0.15f);
                        goTimer = 0.65f;
                    }
                }
            }
            else if (goTimer > 0f)
            {
                goTimer -= Time.deltaTime;
                if (goTimer <= 0f && countdownText != null)
                    countdownText.gameObject.SetActive(false);
            }

            if (autoRaceLaunched) throttle = 1f;
        }
        else if (isHoldingGas)
            throttle = 1f;
        else if (Input.GetMouseButton(0))
            throttle = 1f;
        else
            throttle = Mathf.Max(0f, Input.GetAxis("Vertical"));

        if (car != null) car.ApplyThrottle(throttle);

        if (car != null) maxSpeed = Mathf.Max(maxSpeed, car.velocity * 3.6f);

        if (speedText != null && car != null)
            speedText.text = Mathf.Round(car.velocity * 3.6f).ToString();

        if (gearText != null && car != null)
            gearText.text = car.currentGear.ToString();

        if (rpmBar != null && car != null)
            rpmBar.value = car.currentRPM / car.maxRPM;

        if (nitroBar != null && nitroBoost != null)
            nitroBar.value = nitroBoost.NitroLevel / 100f;

        if (!finishShown && raceTimer != null && raceTimer.IsFinished)
        {
            finishShown = true;
            if (finishPanel != null)
            {
                finishPanel.SetActive(true);
                if (finishTimeText != null)
                    finishTimeText.text = raceTimer.FinalTime.ToString("F2");
                if (finishSpeedText != null)
                    finishSpeedText.text = "TOP SPEED   " + Mathf.Round(maxSpeed) + " km/h";
            }
        }
    }
}
