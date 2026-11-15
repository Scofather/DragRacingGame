using UnityEngine;

public class NitroBoost : MonoBehaviour
{
    public CarPhysics car;
    public float boostMultiplier = 1.6f;
    public float drainRate       = 20f;
    public float rechargeRate    = 8f;
    public float startingNitro   = 100f;

    public bool  IsActive   => isActive;
    public float NitroLevel => nitroLevel;

    private float nitroLevel;
    private bool  isActive;
    private float baseMaxVelocity;
    private float baseMaxTorque;

    private void Awake()
    {
        nitroLevel = startingNitro;
    }

    private void Start()
    {
        if (car != null)
        {
            baseMaxVelocity = car.maxVelocity;
            baseMaxTorque   = car.maxTorque;
        }
    }

    public void ActivateNitro()
    {
        if (nitroLevel <= 0f || car == null || isActive) return;
        isActive        = true;
        car.maxVelocity = baseMaxVelocity * boostMultiplier;
        car.maxTorque   = baseMaxTorque   * boostMultiplier;
    }

    public void DeactivateNitro()
    {
        if (!isActive) return;
        isActive = false;
        if (car != null)
        {
            car.maxVelocity = baseMaxVelocity;
            car.maxTorque   = baseMaxTorque;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) ActivateNitro();
        if (Input.GetKeyUp(KeyCode.N))   DeactivateNitro();

        if (isActive)
        {
            nitroLevel -= drainRate * Time.deltaTime;
            if (nitroLevel <= 0f)
            {
                nitroLevel = 0f;
                DeactivateNitro();
            }
        }
        else if (nitroLevel < startingNitro)
        {
            nitroLevel = Mathf.Min(nitroLevel + rechargeRate * Time.deltaTime, startingNitro);
        }
    }
}
