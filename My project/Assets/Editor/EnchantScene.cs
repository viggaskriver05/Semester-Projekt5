using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Tools > Enchant Scene
// One-click mood upgrades for the forest: post-processing, warmer light + mossy fog,
// and live controls for the pollen particle system that's already in the scene. Each section has its own button; Cmd+Z undoes.
public class EnchantScene : EditorWindow
{
    const string ProfilePath = "Assets/Settings/EnchantedForestProfile.asset";

    // post
    float saturation = 15f, contrast = 12f, bloom = 0.6f, vignette = 0.2f;
    // light & fog
    Color fogColor = new Color(0.44f, 0.55f, 0.52f);
    float fogDensity = -1f;
    Color sunColor = new Color(1f, 0.91f, 0.76f);
    // pollen (the existing particle system, edited live)
    [SerializeField] ParticleSystem pollen;
    Vector2 scroll;

    [MenuItem("Tools/Enchant Scene")]
    static void Open() => GetWindow<EnchantScene>("Enchant Scene");

    void OnEnable() { if (fogDensity < 0) fogDensity = RenderSettings.fogDensity; }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        Header("1. Post-processing");
        saturation = EditorGUILayout.Slider("Saturation", saturation, -30, 50);
        contrast = EditorGUILayout.Slider("Contrast", contrast, -20, 40);
        bloom = EditorGUILayout.Slider("Bloom", bloom, 0, 3);
        vignette = EditorGUILayout.Slider("Vignette (keep low for VR)", vignette, 0, 0.5f);
        if (GUILayout.Button("Apply post-processing")) PostProcessing();

        Header("2. Light & fog");
        fogColor = EditorGUILayout.ColorField("Fog colour", fogColor);
        fogDensity = EditorGUILayout.Slider("Fog density", fogDensity, 0f, 0.15f);
        sunColor = EditorGUILayout.ColorField("Sunlight colour", sunColor);
        if (GUILayout.Button("Apply light & fog")) LightAndFog();

        PollenGUI();

        EditorGUILayout.Space(12);
        if (GUILayout.Button("Apply everything", GUILayout.Height(32)))
        { PostProcessing(); LightAndFog(); }
        EditorGUILayout.EndScrollView();
    }

    static void Header(string t) { EditorGUILayout.Space(8); EditorGUILayout.LabelField(t, EditorStyles.boldLabel); }

    // ---------- 1 ----------
    void PostProcessing()
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            profile = CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }
        Undo.RecordObject(profile, "Enchant post-processing");

        var tm = Get<Tonemapping>(profile); tm.mode.Override(TonemappingMode.ACES);
        var ca = Get<ColorAdjustments>(profile);
        ca.postExposure.Override(0.25f);
        ca.contrast.Override(contrast);
        ca.saturation.Override(saturation);
        ca.colorFilter.Override(new Color(1f, 0.97f, 0.93f));
        var bl = Get<Bloom>(profile);
        bl.threshold.Override(0.85f); bl.intensity.Override(bloom); bl.scatter.Override(0.7f);
        bl.tint.Override(new Color(1f, 0.95f, 0.85f));
        var vg = Get<Vignette>(profile);
        vg.intensity.Override(vignette); vg.smoothness.Override(0.45f);
        var sm = Get<ShadowsMidtonesHighlights>(profile);
        sm.shadows.Override(new Vector4(0.92f, 1f, 1.02f, 0f)); // slightly teal shadows
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        var go = GameObject.Find("Enchanted Volume");
        if (go == null)
        {
            go = new GameObject("Enchanted Volume");
            Undo.RegisterCreatedObjectUndo(go, "Enchant post-processing");
        }
        if (!go.TryGetComponent<Volume>(out var vol)) vol = Undo.AddComponent<Volume>(go);
        vol.isGlobal = true; vol.priority = 10; vol.sharedProfile = profile;

        foreach (var cam in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var data = cam.GetUniversalAdditionalCameraData();
            Undo.RecordObject(data, "Enchant post-processing");
            data.renderPostProcessing = true;
            EditorUtility.SetDirty(data);
        }
        Debug.Log("Enchant: post-processing applied.");
    }

    static T Get<T>(VolumeProfile p) where T : VolumeComponent
    {
        if (!p.TryGet<T>(out var c))
        {
            c = p.Add<T>(true);
            c.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(c, p);
        }
        c.active = true;
        return c;
    }

    // ---------- 2 ----------
    void LightAndFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        var sun = RenderSettings.sun;
        if (sun == null)
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { sun = l; break; }
        if (sun != null)
        {
            Undo.RecordObject(sun, "Enchant light");
            sun.color = sunColor;
            sun.shadows = LightShadows.Soft;
            EditorUtility.SetDirty(sun);
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Enchant: light & fog applied.");
    }

    // ---------- 3 ----------
    // Shows the most useful settings of the pollen particle system that's already in the scene.
    // Changes apply straight away (Cmd+Z undoes); everything else is still in the normal Inspector.
    static ParticleSystem FindPollen()
    {
        var sel = Selection.activeGameObject;
        if (sel != null && sel.TryGetComponent<ParticleSystem>(out var selected)) return selected;
        var all = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var ps in all) if (ps.name.ToLower().Contains("pollen")) return ps;
        return all.Length == 1 ? all[0] : null;
    }

    void PollenGUI()
    {
        Header("3. Pollen");
        if (pollen == null) pollen = FindPollen();

        EditorGUILayout.BeginHorizontal();
        pollen = (ParticleSystem)EditorGUILayout.ObjectField("Particle system", pollen, typeof(ParticleSystem), true);
        if (GUILayout.Button("Find", GUILayout.Width(50))) pollen = FindPollen();
        EditorGUILayout.EndHorizontal();
        if (pollen == null)
        {
            EditorGUILayout.HelpBox("Drag the pollen particle system in here, or select it in the Hierarchy and click Find.", MessageType.Info);
            return;
        }

        var ps = pollen;
        var rend = ps.GetComponent<ParticleSystemRenderer>();
        var main = ps.main; var em = ps.emission; var shape = ps.shape; var noise = ps.noise;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Amount", EditorStyles.miniBoldLabel);
        int maxP = EditorGUILayout.IntSlider("Max specks", main.maxParticles, 10, 5000);
        var rate = CurveField("Specks per second", em.rateOverTime, 0f, 1000f);
        var life = CurveField("Lifetime (s)", main.startLifetime, 0.1f, 60f);

        EditorGUILayout.LabelField("Look", EditorStyles.miniBoldLabel);
        var size = CurveField("Size", main.startSize, 0.001f, 5f);
        Color colour = main.startColor.color;
        bool plainColour = main.startColor.mode == ParticleSystemGradientMode.Color;
        if (plainColour) colour = EditorGUILayout.ColorField(new GUIContent("Colour (HDR = glow)"), colour, true, true, true);
        else EditorGUILayout.LabelField("Colour", "uses a gradient, edit in the Inspector");
        var mat = (Material)EditorGUILayout.ObjectField("Material", rend.sharedMaterial, typeof(Material), false);

        EditorGUILayout.LabelField("Movement", EditorStyles.miniBoldLabel);
        var speed = CurveField("Speed", main.startSpeed, 0f, 20f);
        bool noiseOn = EditorGUILayout.Toggle("Swirl (noise)", noise.enabled);
        var noiseStr = noise.strength;
        if (noiseOn) noiseStr = CurveField("   Swirl strength", noise.strength, 0f, 20f);
        var grav = CurveField("Gravity (- = rises)", main.gravityModifier, -1f, 1f);
        bool world = EditorGUILayout.Toggle(new GUIContent("Stay in place when walking", "World simulation space"),
                                            main.simulationSpace == ParticleSystemSimulationSpace.World);

        EditorGUILayout.LabelField("Area", EditorStyles.miniBoldLabel);
        Vector3 area = shape.scale; float radius = shape.radius;
        bool boxy = shape.shapeType == ParticleSystemShapeType.Box || shape.shapeType == ParticleSystemShapeType.BoxShell || shape.shapeType == ParticleSystemShapeType.BoxEdge;
        if (boxy) area = EditorGUILayout.Vector3Field("Box size (m)", area);
        else radius = EditorGUILayout.FloatField("Radius (m)", radius);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(ps, "Edit pollen");
            main.maxParticles = maxP;
            em.rateOverTime = rate;
            main.startLifetime = life;
            main.startSize = size;
            if (plainColour) main.startColor = colour;
            main.startSpeed = speed;
            noise.enabled = noiseOn; if (noiseOn) noise.strength = noiseStr;
            main.gravityModifier = grav;
            main.simulationSpace = world ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
            if (boxy) shape.scale = area; else shape.radius = radius;
            if (mat != rend.sharedMaterial) { Undo.RecordObject(rend, "Edit pollen"); rend.sharedMaterial = mat; EditorUtility.SetDirty(rend); }
            EditorUtility.SetDirty(ps);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ps.gameObject.scene);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Restart preview")) { ps.Clear(true); ps.Play(true); }
        if (GUILayout.Button("Select in Hierarchy")) { Selection.activeGameObject = ps.gameObject; EditorGUIUtility.PingObject(ps.gameObject); }
        EditorGUILayout.EndHorizontal();
    }

    // one slider for a single value, two boxes for "random between", read-only if it's a curve
    static ParticleSystem.MinMaxCurve CurveField(string label, ParticleSystem.MinMaxCurve c, float lo, float hi)
    {
        switch (c.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return new ParticleSystem.MinMaxCurve(EditorGUILayout.Slider(label, c.constant, lo, hi));
            case ParticleSystemCurveMode.TwoConstants:
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                EditorGUIUtility.labelWidth = 32;
                float a = EditorGUILayout.FloatField("min", c.constantMin);
                float b = EditorGUILayout.FloatField("max", c.constantMax);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                return new ParticleSystem.MinMaxCurve(Mathf.Clamp(a, lo, hi), Mathf.Clamp(b, lo, hi));
            default:
                EditorGUILayout.LabelField(label, "uses a curve, edit in the Inspector");
                return c;
        }
    }
}
