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

    [Header("Pre-Race Menu")]
    public GameObject startPanel;
    public Text weatherChoiceText;
    public Text difficultyChoiceText;
    public Text menuBestText;
    public WeatherSystem weatherSystem;

    [Header("Countdown")]
    public Text countdownText;
    public float countdownDelay = 3f;

    [Header("Post-Race")]
    public GameObject finishPanel;
    public Text finishTimeText;
    public Text finishSpeedText;
    public Text finishResultText;
    public Text finishRivalText;
    public Text finishBestText;
    public float stopDeceleration = 40f;

    [Header("Nitro")]
    public NitroBoost nitroBoost;
    public Slider nitroBar;

    [Header("AI")]
    public AIOpponent aiOpponent;
    public Text rivalText;

    [Header("Auto Race (demo)")]
    public bool autoRace = false;

    private const string BestTimeKey   = "BestQuarterMile";
    private const string DifficultyKey = "Difficulty";
    private const string WeatherKey    = "Weather";

    private static readonly string[] DifficultyNames    = { "EASY", "MEDIUM", "HARD" };
    private static readonly float[]  DifficultyBias     = { 0.90f, 0.97f, 1.04f };
    private static readonly float[]  DifficultyReaction = { 0.55f, 0.35f, 0.20f };
    private static readonly string[] WeatherNames       = { "SUNNY", "RAIN", "SNOW" };

    private int   difficulty = 1;
    private int   weather    = 0;
    private bool  inMenu;
    private float countdownTimer = 0f;
    private bool  launched = false;
    private float goTimer = 0f;

    private bool  isHoldingGas = false;
    private float maxSpeed = 0f;
    private bool  finishShown = false;
    private bool  rivalTimeShown = false;

    public void OnGasPedalDown() => isHoldingGas = true;
    public void OnGasPedalUp()   => isHoldingGas = false;

    private void Start()
    {
        difficulty = Mathf.Clamp(PlayerPrefs.GetInt(DifficultyKey, 1), 0, DifficultyNames.Length - 1);
        weather    = Mathf.Clamp(PlayerPrefs.GetInt(WeatherKey, 0), 0, WeatherNames.Length - 1);
        ApplyWeather();

        inMenu = startPanel != null;
        if (inMenu)
        {
            startPanel.SetActive(true);
            if (countdownText != null) countdownText.gameObject.SetActive(false);
            if (menuBestText != null)
            {
                float best = PlayerPrefs.GetFloat(BestTimeKey, 0f);
                menuBestText.text = best > 0f ? string.Format("BEST   {0:F2}s", best) : "";
            }
            RefreshMenu();
        }
        else
        {
            ApplyDifficulty();
        }
    }

    public void CycleWeather()
    {
        if (!inMenu) return;
        weather = (weather + 1) % WeatherNames.Length;
        ApplyWeather();
        RefreshMenu();
    }

    public void CycleDifficulty()
    {
        if (!inMenu) return;
        difficulty = (difficulty + 1) % DifficultyNames.Length;
        RefreshMenu();
    }

    public void BeginRace()
    {
        if (!inMenu) return;
        inMenu = false;
        startPanel.SetActive(false);
        PlayerPrefs.SetInt(DifficultyKey, difficulty);
        PlayerPrefs.SetInt(WeatherKey, weather);
        PlayerPrefs.Save();
        ApplyDifficulty();
    }

    private void ApplyWeather()
    {
        if (weatherSystem != null)
            weatherSystem.currentWeather = (WeatherSystem.WeatherType)weather;
    }

    private void ApplyDifficulty()
    {
        if (aiOpponent != null)
            aiOpponent.Configure(DifficultyBias[difficulty], DifficultyReaction[difficulty]);
    }

    private void RefreshMenu()
    {
        if (weatherChoiceText != null)    weatherChoiceText.text    = "WEATHER:  " + WeatherNames[weather];
        if (difficultyChoiceText != null) difficultyChoiceText.text = "RIVAL:  " + DifficultyNames[difficulty];
    }

    private void Update()
    {
        if (inMenu)
        {
            if (Input.GetKeyDown(KeyCode.W)) CycleWeather();
            if (Input.GetKeyDown(KeyCode.D)) CycleDifficulty();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
                BeginRace();
            return;
        }

        bool finished = raceTimer != null && raceTimer.IsFinished;

        if (finished && (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space)))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        float throttle = 0f;

        if (!launched)
        {
            countdownTimer += Time.deltaTime;

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                float r = countdownDelay - countdownTimer;
                if      (r > 2f) { countdownText.text = "3"; countdownText.color = Color.white; }
                else if (r > 1f) { countdownText.text = "2"; countdownText.color = Color.white; }
                else             { countdownText.text = "1"; countdownText.color = new Color(1f, 0.35f, 0.35f); }
            }

            if (countdownTimer >= countdownDelay)
            {
                launched = true;
                if (raceTimer != null) raceTimer.StartRace();
                if (countdownText != null)
                {
                    countdownText.text  = "GO!";
                    countdownText.color = new Color(0.15f, 0.9f, 0.15f);
                    goTimer = 0.65f;
                }
            }
        }
        else
        {
            if (goTimer > 0f)
            {
                goTimer -= Time.deltaTime;
                if (goTimer <= 0f && countdownText != null)
                    countdownText.gameObject.SetActive(false);
            }

            if (finished)
                throttle = 0f;
            else if (autoRace || isHoldingGas || Input.GetMouseButton(0))
                throttle = 1f;
            else
                throttle = Mathf.Max(0f, Input.GetAxis("Vertical"));
        }

        if (car != null)
        {
            car.ApplyThrottle(throttle);
            if (finished)
                car.velocity = Mathf.Max(0f, car.velocity - stopDeceleration * Time.deltaTime);

            maxSpeed = Mathf.Max(maxSpeed, car.velocity * 3.6f);

            if (speedText != null) speedText.text = Mathf.Round(car.velocity * 3.6f).ToString();
            if (gearText != null)  gearText.text  = car.currentGear.ToString();
            if (rpmBar != null)    rpmBar.value   = car.currentRPM / car.maxRPM;
        }

        if (nitroBar != null && nitroBoost != null)
            nitroBar.value = nitroBoost.NitroLevel / nitroBoost.startingNitro;

        if (rivalText != null && aiOpponent != null && car != null)
        {
            if (finished)
            {
                rivalText.gameObject.SetActive(false);
            }
            else if (aiOpponent.HasFinished)
            {
                rivalText.text = string.Format("RIVAL FINISHED  {0:F2}s", aiOpponent.FinishTime);
            }
            else
            {
                float delta = aiOpponent.transform.position.z - car.transform.position.z;
                if (delta > 1f)
                    rivalText.text = string.Format("RIVAL  +{0:F0}m", delta);
                else if (delta < -1f)
                    rivalText.text = string.Format("RIVAL  -{0:F0}m", Mathf.Abs(delta));
                else
                    rivalText.text = "NECK & NECK";
            }
        }

        if (!finishShown && finished)
        {
            finishShown = true;

            // The rival crossing first is a loss; a lost run is not saved as a best time.
            bool  lost    = aiOpponent != null && aiOpponent.HasFinished;
            float best    = PlayerPrefs.GetFloat(BestTimeKey, 0f);
            bool  newBest = !lost && (best <= 0f || raceTimer.FinalTime < best);
            if (newBest)
            {
                PlayerPrefs.SetFloat(BestTimeKey, raceTimer.FinalTime);
                PlayerPrefs.Save();
            }

            if (finishPanel != null)
            {
                finishPanel.SetActive(true);
                if (finishTimeText != null)
                    finishTimeText.text = raceTimer.FinalTime.ToString("F2");
                if (finishSpeedText != null)
                    finishSpeedText.text = "TOP SPEED   " + Mathf.Round(maxSpeed) + " km/h";
                if (finishBestText != null)
                    finishBestText.text = newBest ? "NEW BEST!" : string.Format("BEST   {0:F2}s", best);

                if (finishResultText != null)
                {
                    if (aiOpponent == null)
                    {
                        finishResultText.gameObject.SetActive(false);
                    }
                    else
                    {
                        bool won = !lost;
                        finishResultText.text  = won ? "YOU WIN" : "YOU LOSE";
                        finishResultText.color = won ? new Color(0.15f, 0.9f, 0.15f) : new Color(1f, 0.25f, 0.2f);
                    }
                }
                if (finishRivalText != null)
                    finishRivalText.text = aiOpponent != null ? "RIVAL   STILL RACING" : "";
            }
        }

        if (finishShown && !rivalTimeShown && finishRivalText != null && aiOpponent != null && aiOpponent.HasFinished)
        {
            rivalTimeShown = true;
            finishRivalText.text = string.Format("RIVAL   {0:F2}s", aiOpponent.FinishTime);
        }
    }
}
