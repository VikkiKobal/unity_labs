using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class Lab5Environment
{
    [MenuItem("Tools/Lab 5/Setup Beautiful Environment")]
    static void SetupEnvironment()
    {
        SetupSkybox();
        SetupLighting();
        SetupFog();
        SetupPostProcessing();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[Lab5] Environment setup complete!");
    }

    // ── 1. SKYBOX ────────────────────────────────────────────────────────────
    static void SetupSkybox()
    {
        // Use Unity's built-in procedural skybox (sun + atmosphere)
        var skyMat = new Material(Shader.Find("Skybox/Procedural"));
        skyMat.name = "GameSkybox";

        // Sunset/golden hour palette
        skyMat.SetFloat("_SunSize",           0.04f);
        skyMat.SetFloat("_SunSizeConvergence", 5f);
        skyMat.SetFloat("_AtmosphereThickness", 1.1f);
        skyMat.SetColor("_SkyTint",   new Color(0.35f, 0.55f, 0.95f)); // deep blue sky
        skyMat.SetColor("_GroundColor", new Color(0.25f, 0.22f, 0.18f)); // warm earth
        skyMat.SetFloat("_Exposure",  1.3f);

        // Save material to Assets
        const string matPath = "Assets/Materials/GameSkybox.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(matPath) == null)
            AssetDatabase.CreateAsset(skyMat, matPath);
        else
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            EditorUtility.CopySerialized(skyMat, existing);
            skyMat = existing;
        }

        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.2f;
        DynamicGI.UpdateEnvironment();

        Debug.Log("[Lab5] Skybox set.");
    }

    // ── 2. DIRECTIONAL LIGHT ─────────────────────────────────────────────────
    static void SetupLighting()
    {
        var sun = GameObject.Find("Directional Light");
        if (sun == null)
        {
            sun = new GameObject("Directional Light");
            sun.AddComponent<Light>();
        }

        var light = sun.GetComponent<Light>();
        light.type      = LightType.Directional;
        light.color     = new Color(1f, 0.95f, 0.82f);   // warm sunlight
        light.intensity = 1.4f;
        light.shadows   = LightShadows.Soft;
        light.shadowStrength = 0.7f;

        // Angle: slightly from above-side (like afternoon sun)
        sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        RenderSettings.sun = light;

        Debug.Log("[Lab5] Sun light configured.");
    }

    // ── 3. FOG ───────────────────────────────────────────────────────────────
    static void SetupFog()
    {
        RenderSettings.fog          = true;
        RenderSettings.fogMode      = FogMode.Linear;
        RenderSettings.fogColor     = new Color(0.55f, 0.65f, 0.85f); // soft blue-grey haze
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance   = 120f;

        Debug.Log("[Lab5] Fog configured.");
    }

    // ── 4. POST-PROCESSING VOLUME (URP) ──────────────────────────────────────
    static void SetupPostProcessing()
    {
        // Remove old PP volume if exists
        var old = GameObject.Find("PostProcessing_Global");
        if (old != null) Object.DestroyImmediate(old);

        var ppGO = new GameObject("PostProcessing_Global");

        var volume = ppGO.AddComponent<Volume>();
        volume.isGlobal  = true;
        volume.priority  = 10;

        // Create profile asset
        const string profilePath = "Assets/Settings/PP_GameProfile.asset";
        VolumeProfile profile;
        var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (existing != null)
            profile = existing;
        else
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        volume.sharedProfile = profile;

        // ── Bloom ──
        if (!profile.TryGet<Bloom>(out var bloom))
            bloom = profile.Add<Bloom>(false);
        bloom.active         = true;
        bloom.intensity.value      = 0.6f;
        bloom.threshold.value      = 0.9f;
        bloom.scatter.value        = 0.5f;
        bloom.intensity.overrideState   = true;
        bloom.threshold.overrideState   = true;
        bloom.scatter.overrideState     = true;

        // ── Color Adjustments ──
        if (!profile.TryGet<ColorAdjustments>(out var ca))
            ca = profile.Add<ColorAdjustments>(false);
        ca.active = true;
        ca.contrast.value          =  12f;
        ca.saturation.value        =  18f;
        ca.postExposure.value      =  0.1f;
        ca.colorFilter.value       = new Color(1f, 0.97f, 0.92f); // slight warm tint
        ca.contrast.overrideState       = true;
        ca.saturation.overrideState     = true;
        ca.postExposure.overrideState   = true;
        ca.colorFilter.overrideState    = true;

        // ── Vignette ──
        if (!profile.TryGet<Vignette>(out var vig))
            vig = profile.Add<Vignette>(false);
        vig.active           = true;
        vig.intensity.value  = 0.28f;
        vig.smoothness.value = 0.4f;
        vig.intensity.overrideState  = true;
        vig.smoothness.overrideState = true;

        // ── Tonemapping ──
        if (!profile.TryGet<Tonemapping>(out var tm))
            tm = profile.Add<Tonemapping>(false);
        tm.active = true;
        tm.mode.value = TonemappingMode.ACES;
        tm.mode.overrideState = true;

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        Debug.Log("[Lab5] Post-processing (Bloom, Color Grade, Vignette, ACES Tonemapping) applied.");
    }
}
