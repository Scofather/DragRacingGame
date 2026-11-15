using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MasterAutomator
{
    [MenuItem("Tools/Build Complete Drag Racing Game")]
    public static void BuildScene()
    {
        // ── Idempotent cleanup ──────────────────────────────────────────────
        string[] rootNames = { "Drag Strip", "Player Car", "UI Canvas", "EventSystem", "Game Manager", "Directional Light" };
        foreach (string n in rootNames)
        {
            var ex = GameObject.Find(n);
            if (ex != null) Object.DestroyImmediate(ex);
        }

        // ── Shader ──────────────────────────────────────────────────────────
        Shader lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        Material Mat(Color c, float metallic = 0f, float smooth = 0.5f)
        {
            var m = new Material(lit);
            m.color = c;
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smooth);
            return m;
        }

        // ── Lighting ────────────────────────────────────────────────────────
        var existingLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in existingLights)
            if (l.type == LightType.Directional) Object.DestroyImmediate(l.gameObject);

        var lightGO = new GameObject("Directional Light");
        var dirLight = lightGO.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(1.0f, 0.93f, 0.78f);
        dirLight.intensity = 1.3f;
        dirLight.transform.eulerAngles = new Vector3(42f, -25f, 0f);
        dirLight.shadows = LightShadows.Soft;

        // ── 1. TRACK ────────────────────────────────────────────────────────
        var dragStrip = new GameObject("Drag Strip");

        // Asphalt
        var asphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        asphalt.name = "Asphalt";
        asphalt.transform.SetParent(dragStrip.transform, false);
        asphalt.transform.position = new Vector3(0f, -0.5f, 250f);
        asphalt.transform.localScale = new Vector3(16f, 1f, 600f);
        asphalt.GetComponent<Renderer>().material = Mat(new Color(0.14f, 0.14f, 0.14f), 0f, 0.15f);

        // Centre line dashes
        var dashMat = Mat(Color.yellow, 0f, 0.4f);
        for (int z = 10; z < 400; z += 15)
        {
            var dash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(dash.GetComponent<Collider>());
            dash.name = "Dash";
            dash.transform.SetParent(dragStrip.transform, false);
            dash.transform.position = new Vector3(0f, 0.01f, z);
            dash.transform.localScale = new Vector3(0.18f, 0.02f, 5f);
            dash.GetComponent<Renderer>().material = dashMat;
        }

        // Starting line (white stripes)
        var whiteMat = Mat(Color.white, 0f, 0.3f);
        for (int s = -3; s <= 3; s++)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(stripe.GetComponent<Collider>());
            stripe.name = "StartStripe";
            stripe.transform.SetParent(dragStrip.transform, false);
            stripe.transform.position = new Vector3(s * 2.0f, 0.01f, 0f);
            stripe.transform.localScale = new Vector3(0.9f, 0.02f, 1.5f);
            stripe.GetComponent<Renderer>().material = s % 2 == 0 ? whiteMat : Mat(new Color(0.14f, 0.14f, 0.14f));
        }

        // Side barriers (concrete walls)
        var barrierMat = Mat(new Color(0.82f, 0.82f, 0.82f), 0.1f, 0.3f);
        for (int side = -1; side <= 1; side += 2)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(wall.GetComponent<Collider>());
            wall.name = side < 0 ? "BarrierLeft" : "BarrierRight";
            wall.transform.SetParent(dragStrip.transform, false);
            wall.transform.position = new Vector3(side * 8.2f, 0.45f, 250f);
            wall.transform.localScale = new Vector3(0.4f, 0.9f, 600f);
            wall.GetComponent<Renderer>().material = barrierMat;

            // Red/white stripe on top of barrier
            for (int z = 0; z < 600; z += 6)
            {
                var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(stripe.GetComponent<Collider>());
                stripe.transform.SetParent(dragStrip.transform, false);
                stripe.transform.position = new Vector3(side * 8.2f, 0.95f, z);
                stripe.transform.localScale = new Vector3(0.42f, 0.12f, 2.5f);
                stripe.GetComponent<Renderer>().material = z % 12 < 6
                    ? Mat(new Color(0.9f, 0.1f, 0.1f))
                    : whiteMat;
            }
        }

        // Starting tree (Christmas tree) — left side
        var treeMat = Mat(new Color(0.12f, 0.12f, 0.12f), 0.5f, 0.6f);
        for (int side = -1; side <= 1; side += 2)
        {
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(pole.GetComponent<Collider>());
            pole.transform.SetParent(dragStrip.transform, false);
            pole.transform.position = new Vector3(side * 6f, 2.5f, -3f);
            pole.transform.localScale = new Vector3(0.15f, 2.5f, 0.15f);
            pole.GetComponent<Renderer>().material = treeMat;
            Color[] bulbColors = { new Color(1f,0.3f,0.1f), new Color(1f,0.9f,0.1f), new Color(1f,0.9f,0.1f), new Color(0.1f,0.9f,0.2f) };
            for (int b = 0; b < 4; b++)
            {
                var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.DestroyImmediate(bulb.GetComponent<Collider>());
                bulb.transform.SetParent(dragStrip.transform, false);
                bulb.transform.position = new Vector3(side * 6f + side * 0.4f, 4.3f - b * 0.65f, -3f);
                bulb.transform.localScale = Vector3.one * 0.28f;
                bulb.GetComponent<Renderer>().material = Mat(bulbColors[b] * 0.6f);
            }
        }

        // Finish line trigger
        var finishLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finishLine.name = "Finish Line Trigger";
        finishLine.transform.SetParent(dragStrip.transform, false);
        finishLine.transform.position = new Vector3(0f, 0.01f, 402.33f);
        finishLine.transform.localScale = new Vector3(16f, 0.02f, 1.5f);
        // Checkered finish line
        finishLine.GetComponent<Renderer>().material = Mat(Color.white);
        Object.DestroyImmediate(finishLine.GetComponent<Collider>());
        var finishTrigger = finishLine.AddComponent<BoxCollider>();
        finishTrigger.isTrigger = true;
        finishTrigger.size = new Vector3(20f, 10f, 1f);

        // Checkerboard squares on finish line
        for (int i = -4; i <= 4; i++)
        {
            var sq = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(sq.GetComponent<Collider>());
            sq.transform.SetParent(dragStrip.transform, false);
            sq.transform.position = new Vector3(i * 1.8f, 0.02f, 402.33f);
            sq.transform.localScale = new Vector3(0.9f, 0.02f, 1.5f);
            sq.GetComponent<Renderer>().material = i % 2 == 0 ? Mat(Color.black) : whiteMat;
        }

        // Finish gantry
        var gantryMat = Mat(new Color(0.85f, 0.85f, 0.85f), 0.6f, 0.7f);
        for (int side = -1; side <= 1; side += 2)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(pillar.GetComponent<Collider>());
            pillar.transform.SetParent(dragStrip.transform, false);
            pillar.transform.position = new Vector3(side * 7.5f, 3.5f, 402.33f);
            pillar.transform.localScale = new Vector3(0.35f, 3.5f, 0.35f);
            pillar.GetComponent<Renderer>().material = gantryMat;
        }
        var crossbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(crossbar.GetComponent<Collider>());
        crossbar.transform.SetParent(dragStrip.transform, false);
        crossbar.transform.position = new Vector3(0f, 7.2f, 402.33f);
        crossbar.transform.localScale = new Vector3(15.5f, 0.4f, 0.4f);
        crossbar.GetComponent<Renderer>().material = Mat(new Color(0.1f, 0.25f, 0.85f), 0.3f, 0.7f);
        // "FINISH" sign panel on gantry
        var signPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(signPanel.GetComponent<Collider>());
        signPanel.transform.SetParent(dragStrip.transform, false);
        signPanel.transform.position = new Vector3(0f, 6.5f, 402.33f);
        signPanel.transform.localScale = new Vector3(5f, 0.9f, 0.15f);
        signPanel.GetComponent<Renderer>().material = Mat(new Color(0.9f, 0.75f, 0.0f), 0f, 0.8f);

        // ── 2. PLAYER CAR ───────────────────────────────────────────────────
        var playerCar = new GameObject("Player Car");
        playerCar.transform.position = new Vector3(0f, 0f, 0f);

        var carCollider = playerCar.AddComponent<BoxCollider>();
        carCollider.center = new Vector3(0f, 0.6f, 0f);
        carCollider.size = new Vector3(1.8f, 1.4f, 4.0f);

        var rb = playerCar.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var carPhysics = playerCar.AddComponent<CarPhysics>();

        // Car body — low, wide racing car
        var bodyMat = Mat(new Color(0.92f, 0.08f, 0.05f), 0.25f, 0.75f);

        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.name = "Body";
        body.transform.SetParent(playerCar.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        body.transform.localScale = new Vector3(2.0f, 0.55f, 4.5f);
        body.GetComponent<Renderer>().material = bodyMat;

        // Cabin / cockpit
        var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(cabin.GetComponent<Collider>());
        cabin.name = "Cabin";
        cabin.transform.SetParent(playerCar.transform, false);
        cabin.transform.localPosition = new Vector3(0f, 0.84f, -0.3f);
        cabin.transform.localScale = new Vector3(1.55f, 0.5f, 2.0f);
        cabin.GetComponent<Renderer>().material = Mat(new Color(0.07f, 0.07f, 0.09f), 0.1f, 0.9f);

        // Front splitter
        var splitter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(splitter.GetComponent<Collider>());
        splitter.name = "Splitter";
        splitter.transform.SetParent(playerCar.transform, false);
        splitter.transform.localPosition = new Vector3(0f, 0.1f, 2.35f);
        splitter.transform.localScale = new Vector3(2.15f, 0.1f, 0.4f);
        splitter.GetComponent<Renderer>().material = Mat(new Color(0.08f, 0.08f, 0.08f), 0f, 0.3f);

        // Rear wing
        var rearWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(rearWing.GetComponent<Collider>());
        rearWing.name = "RearWing";
        rearWing.transform.SetParent(playerCar.transform, false);
        rearWing.transform.localPosition = new Vector3(0f, 1.15f, -2.0f);
        rearWing.transform.localScale = new Vector3(2.3f, 0.09f, 0.55f);
        rearWing.GetComponent<Renderer>().material = Mat(new Color(0.08f, 0.08f, 0.08f), 0f, 0.4f);

        // Wing pillars
        for (int s = -1; s <= 1; s += 2)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(pillar.GetComponent<Collider>());
            pillar.name = "WingPillar";
            pillar.transform.SetParent(playerCar.transform, false);
            pillar.transform.localPosition = new Vector3(s * 0.85f, 0.88f, -2.0f);
            pillar.transform.localScale = new Vector3(0.1f, 0.56f, 0.1f);
            pillar.GetComponent<Renderer>().material = Mat(new Color(0.08f, 0.08f, 0.08f));
        }

        // Number decal strip on body sides
        var accentMat = Mat(Color.white, 0f, 0.5f);
        for (int s = -1; s <= 1; s += 2)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(stripe.GetComponent<Collider>());
            stripe.name = "Stripe";
            stripe.transform.SetParent(playerCar.transform, false);
            stripe.transform.localPosition = new Vector3(s * 1.02f, 0.4f, 0f);
            stripe.transform.localScale = new Vector3(0.02f, 0.25f, 2.5f);
            stripe.GetComponent<Renderer>().material = accentMat;
        }

        // Wheels (Cylinder, rotated to lay flat)
        var wheelMat = Mat(new Color(0.07f, 0.07f, 0.07f), 0f, 0.2f);
        var rimMat   = Mat(new Color(0.78f, 0.78f, 0.80f), 0.9f, 0.9f);
        Vector3[] wheelPos  = { new Vector3(-1.05f,0.32f,1.5f), new Vector3(1.05f,0.32f,1.5f),
                                new Vector3(-1.05f,0.32f,-1.5f), new Vector3(1.05f,0.32f,-1.5f) };
        string[]  wheelName = { "WheelFL","WheelFR","WheelRL","WheelRR" };
        for (int i = 0; i < 4; i++)
        {
            var tire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(tire.GetComponent<Collider>());
            tire.name = wheelName[i];
            tire.transform.SetParent(playerCar.transform, false);
            tire.transform.localPosition = wheelPos[i];
            tire.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            tire.transform.localScale = new Vector3(0.32f, 0.17f, 0.32f);
            tire.GetComponent<Renderer>().material = wheelMat;

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(rim.GetComponent<Collider>());
            rim.name = "Rim";
            rim.transform.SetParent(tire.transform, false);
            rim.transform.localPosition = Vector3.zero;
            rim.transform.localScale = new Vector3(0.62f, 1.05f, 0.62f);
            rim.GetComponent<Renderer>().material = rimMat;
        }

        // Tire smoke particles
        var smokeSystems = new ParticleSystem[2];
        string[] smokeNames   = { "SmokeLeft", "SmokeRight" };
        Vector3[] smokeOffsets = { new Vector3(-0.5f, 0f, -2.0f), new Vector3(0.5f, 0f, -2.0f) };
        for (int i = 0; i < 2; i++)
        {
            var smokeGO = new GameObject(smokeNames[i]);
            smokeGO.transform.SetParent(playerCar.transform, false);
            smokeGO.transform.localPosition = smokeOffsets[i];
            var ps   = smokeGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake  = false;
            main.loop         = true;
            main.startLifetime = 1.8f;
            main.startSpeed   = 2f;
            main.startSize    = 0.6f;
            main.startColor   = new ParticleSystem.MinMaxGradient(
                new Color(0.55f, 0.55f, 0.55f, 0.8f), new Color(0.9f, 0.9f, 0.9f, 0.3f));
            var em = ps.emission; em.enabled = false; em.rateOverTime = 25f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.gray, 0f), new GradientColorKey(Color.white, 1f) },
                         new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(grad);
            smokeSystems[i] = ps;
        }
        carPhysics.tireSmokeParticles = smokeSystems;

        // ── 3. CAMERA ───────────────────────────────────────────────────────
        var camGO = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
        if (Camera.main == null) { camGO.AddComponent<Camera>(); camGO.tag = "MainCamera"; }
        camGO.transform.SetParent(playerCar.transform, false);
        camGO.transform.localPosition    = new Vector3(0f, 1.1f, -5.5f);
        camGO.transform.localEulerAngles = new Vector3(9f, 0f, 0f);

        // ── 4. UI CANVAS ────────────────────────────────────────────────────
        var canvasGO = new GameObject("UI Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Helper — anchors at bottom-left by default
        Image MakePanel(string name, Vector2 pos, Vector2 size, Color col, bool anchorBL = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvasGO.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorBL ? new Vector2(0f, 0f) : new Vector2(0.5f, 0f);
            rt.anchorMax = rt.anchorMin;
            rt.pivot     = rt.anchorMin;
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;
            var img = go.AddComponent<Image>();
            img.color = col;
            return img;
        }

        Text MakeText(string name, string txt, Vector2 anch, Vector2 pos, Vector2 size,
                      int fs, Color col, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvasGO.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anch;
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;
            var t = go.AddComponent<Text>();
            t.text = txt; t.font = font; t.fontSize = fs; t.color = col; t.alignment = align;
            return t;
        }

        var panelBG  = new Color(0f, 0f, 0f, 0.72f);
        var labelCol = new Color(0.65f, 0.65f, 0.65f, 1f);
        var accentOr = new Color(1f, 0.45f, 0.0f, 1f);

        // Speed panel (bottom-left)
        MakePanel("SpeedBG", new Vector2(14f, 14f), new Vector2(200f, 78f), panelBG);
        var speedText = MakeText("Speed Text", "0",
            new Vector2(0f, 0f), new Vector2(22f, 32f), new Vector2(140f, 55f), 54, Color.white);
        MakeText("SpeedLabel", "KM/H",
            new Vector2(0f, 0f), new Vector2(166f, 25f), new Vector2(60f, 30f), 18, labelCol);

        // Gear panel (next to speed)
        MakePanel("GearBG", new Vector2(224f, 14f), new Vector2(82f, 78f), panelBG);
        MakeText("GearLabel", "GEAR",
            new Vector2(0f, 0f), new Vector2(232f, 72f), new Vector2(66f, 20f), 14, labelCol);
        var gearText = MakeText("Gear Text", "1",
            new Vector2(0f, 0f), new Vector2(236f, 18f), new Vector2(66f, 54f), 50, Color.white, TextAnchor.MiddleCenter);

        // RPM bar panel (above speed/gear)
        MakePanel("RPMBG", new Vector2(14f, 102f), new Vector2(292f, 32f), panelBG);
        MakeText("RPMLabel", "RPM",
            new Vector2(0f, 0f), new Vector2(16f, 110f), new Vector2(44f, 18f), 13, accentOr);

        var sliderGO = new GameObject("RPM Bar");
        sliderGO.transform.SetParent(canvasGO.transform, false);
        var sliderRT = sliderGO.AddComponent<RectTransform>();
        sliderRT.anchorMin = sliderRT.anchorMax = sliderRT.pivot = new Vector2(0f, 0f);
        sliderRT.anchoredPosition = new Vector2(62f, 105f);
        sliderRT.sizeDelta         = new Vector2(238f, 24f);
        var rpmSlider = sliderGO.AddComponent<Slider>();
        rpmSlider.minValue = 0f; rpmSlider.maxValue = 1f; rpmSlider.value = 0f; rpmSlider.interactable = false;

        var bgGO = new GameObject("BG"); bgGO.transform.SetParent(sliderGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>(); bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.sizeDelta = Vector2.zero;

        var fillAreaGO = new GameObject("Fill Area"); fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero; fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(3f, 3f); fillAreaRT.offsetMax = new Vector2(-3f, -3f);

        var fillGO = new GameObject("Fill"); fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>(); fillImg.color = accentOr;
        var fillRT  = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one; fillRT.sizeDelta = Vector2.zero;
        rpmSlider.fillRect = fillRT;

        // Timer panel (bottom-centre)
        MakePanel("TimerBG", new Vector2(0f, 14f), new Vector2(260f, 60f), panelBG, false);
        var timerText = MakeText("Timer Text", "Time: 0.00s",
            new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(250f, 48f),
            38, Color.yellow, TextAnchor.MiddleCenter);

        // ── Countdown Text (centre screen, managed by GameManager)
        var countdownGO = new GameObject("Countdown Text");
        countdownGO.transform.SetParent(canvasGO.transform, false);
        var cdRT = countdownGO.AddComponent<RectTransform>();
        cdRT.anchorMin = cdRT.anchorMax = cdRT.pivot = new Vector2(0.5f, 0.5f);
        cdRT.anchoredPosition = new Vector2(0f, 20f);
        cdRT.sizeDelta        = new Vector2(300f, 240f);
        var countdownTxt = countdownGO.AddComponent<Text>();
        countdownTxt.text      = "3";
        countdownTxt.font      = font;
        countdownTxt.fontSize  = 180;
        countdownTxt.color     = Color.white;
        countdownTxt.alignment = TextAnchor.MiddleCenter;

        // ── Finish Panel (initially inactive)
        var finishPanelGO = new GameObject("Finish Panel");
        finishPanelGO.transform.SetParent(canvasGO.transform, false);
        var fpRT = finishPanelGO.AddComponent<RectTransform>();
        fpRT.anchorMin = Vector2.zero; fpRT.anchorMax = Vector2.one;
        fpRT.sizeDelta = Vector2.zero; fpRT.anchoredPosition = Vector2.zero;
        finishPanelGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        Text MakeFP(string name, string txt, float ay, Vector2 size, int fs, Color col)
        {
            var go = new GameObject(name);
            go.transform.SetParent(finishPanelGO.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, ay);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            var t = go.AddComponent<Text>();
            t.text = txt; t.font = font; t.fontSize = fs; t.color = col;
            t.alignment = TextAnchor.MiddleCenter;
            return t;
        }

        MakeFP("FP_Category", "QUARTER MILE",        0.72f, new Vector2(500f, 36f),  20, new Color(0.55f, 0.55f, 0.55f));
        var fpTimeVal = MakeFP("FP_Time",    "0.00",  0.55f, new Vector2(400f, 110f), 96, Color.yellow);
        MakeFP("FP_Suffix",   "SECONDS",             0.42f, new Vector2(400f, 34f),  22, new Color(0.65f, 0.65f, 0.65f));
        var fpSpeed   = MakeFP("FP_Speed",   "TOP SPEED  —", 0.30f, new Vector2(500f, 36f), 24, new Color(0.85f, 0.85f, 0.85f));
        MakeFP("FP_Restart",  "R  /  SPACE  •  RESTART", 0.18f, new Vector2(500f, 32f), 19, new Color(0.38f, 0.38f, 0.38f));
        finishPanelGO.SetActive(false);

        // ── 5. MANAGERS ─────────────────────────────────────────────────────
        var gameManagerGO = new GameObject("Game Manager");
        var gameManager   = gameManagerGO.AddComponent<GameManager>();
        var weatherSystem = gameManagerGO.AddComponent<WeatherSystem>();

        gameManager.car       = carPhysics;
        gameManager.speedText = speedText;
        gameManager.gearText  = gearText;
        gameManager.rpmBar    = rpmSlider;
        gameManager.autoRace  = true;

        weatherSystem.car = carPhysics;

        var raceTimer = finishLine.AddComponent<RaceTimer>();
        raceTimer.timerText      = timerText;
        gameManager.raceTimer    = raceTimer;
        gameManager.countdownText   = countdownTxt;
        gameManager.finishPanel     = finishPanelGO;
        gameManager.finishTimeText  = fpTimeVal;
        gameManager.finishSpeedText = fpSpeed;

        // ── 6. BUTTONS ──────────────────────────────────────────────────────
        Button MakeButton(string label, Vector2 pos)
        {
            var btnGO = new GameObject(label); btnGO.transform.SetParent(canvasGO.transform, false);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(150f, 55f);
            btnGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            var btn = btnGO.AddComponent<Button>();
            var lblGO = new GameObject("Label"); lblGO.transform.SetParent(btnGO.transform, false);
            var lblRT = lblGO.AddComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one; lblRT.sizeDelta = Vector2.zero;
            var lbl = lblGO.AddComponent<Text>();
            lbl.text = label; lbl.font = font; lbl.fontSize = 22; lbl.color = Color.white;
            lbl.alignment = TextAnchor.MiddleCenter;
            return btn;
        }

        var launchBtn = MakeButton("Launch", new Vector2(-10f, 10f));
        var launchET  = launchBtn.gameObject.AddComponent<EventTrigger>();
        var downE = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        downE.callback.AddListener((_) => gameManager.OnGasPedalDown()); launchET.triggers.Add(downE);
        var upE = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        upE.callback.AddListener((_) => gameManager.OnGasPedalUp()); launchET.triggers.Add(upE);

        MakeButton("Shift Up", new Vector2(-10f, 75f)).onClick.AddListener(carPhysics.ShiftUp);
        MakeButton("Shift Down", new Vector2(-10f, 140f)).onClick.AddListener(carPhysics.ShiftDown);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[MasterAutomator] Scene built. Ctrl+S to save.");
    }

    public static void BatchBuild()
    {
        BuildScene();
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene);
    }
}
