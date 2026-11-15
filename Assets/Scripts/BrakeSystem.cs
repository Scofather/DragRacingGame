using UnityEngine;

public class BrakeSystem : MonoBehaviour
{
    public CarPhysics car;
    public float brakeForce = 18f;
    public Renderer[] brakeLights;

    public bool IsBraking => isBraking;

    private bool     isBraking;
    private Material brakeLightMat;

    private static readonly Color LightOff = new Color(0.25f, 0f,    0f, 1f);
    private static readonly Color LightOn  = new Color(1f,    0.05f, 0f, 1f);

    private void Start()
    {
        foreach (var r in brakeLights)
        {
            if (r == null) continue;
            r.material = new Material(r.material);
            r.material.color = LightOff;
        }
    }

    public void Brake()   => isBraking = true;
    public void Release() => isBraking = false;

    private void Update()
    {
        if (Input.GetKey(KeyCode.B) || Input.GetKey(KeyCode.Space))
            isBraking = true;
        else if (!Input.GetKey(KeyCode.B) && !Input.GetKey(KeyCode.Space))
            isBraking = false;

        if (isBraking && car != null && car.velocity > 0f)
            car.velocity = Mathf.Max(0f, car.velocity - brakeForce * Time.deltaTime);

        Color target = isBraking ? LightOn : LightOff;
        foreach (var r in brakeLights)
            if (r != null) r.material.color = target;
    }
}
