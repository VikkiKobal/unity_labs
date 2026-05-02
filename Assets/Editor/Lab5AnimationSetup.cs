using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class Lab5AnimationSetup
{
    const string Downloads       = "/Users/vika_kobal/Downloads";
    const string ModelsPath      = "Assets/Models/Mixamo";
    const string AnimCoin        = "Assets/Animations/Coin";
    const string AnimPlayer      = "Assets/Animations/Player";
    const string CtrlPath        = "Assets/AnimatorControllers";

    static readonly string[] FbxFiles = {
        "Ch22_nonPBR@Walking Left Turn.fbx",
        "Ch22_nonPBR@Running.fbx",
        "Ch22_nonPBR@Jumping.fbx",
        "Ch22_nonPBR@Zombie Death.fbx"
    };

    // ─────────────────────────────────────────
    [MenuItem("Tools/Lab 5/1 - Copy && Import Mixamo FBX")]
    static void Step1_ImportFbx()
    {
        EnsureFolders();
        CopyFbxFiles();
        AssetDatabase.Refresh();
        ConfigureImporters();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ExtractMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Lab5] Step 1 done — FBX imported as Humanoid + materials extracted.");
    }

    [MenuItem("Tools/Lab 5/1b - Extract Materials Only")]
    static void Step1b_ExtractMaterials()
    {
        EnsureFolders();
        if (!AssetDatabase.IsValidFolder("Assets/Models/Mixamo/Materials"))
            AssetDatabase.CreateFolder("Assets/Models/Mixamo", "Materials");
        ExtractMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Lab5] Materials extracted.");
    }

    [MenuItem("Tools/Lab 5/1c - Fix Material Textures")]
    static void Step1c_FixMaterials()
    {
        string matDir = "Assets/Models/Mixamo/Materials";

        // Map: material name → (diffuse tex name, normal tex name)
        var matMap = new System.Collections.Generic.Dictionary<string, (string diff, string norm)>
        {
            { "Ch22_body",          ("Ch22_1001_Diffuse", "Ch22_1001_Normal") },
            { "Ch22_1001_Diffuse",  ("Ch22_1001_Diffuse", "Ch22_1001_Normal") },
            { "Ch22_hair",          ("Ch22_1002_Diffuse", "Ch22_1002_Normal") },
            { "Ch22_1002_Diffuse",  ("Ch22_1002_Diffuse", "Ch22_1002_Normal") },
            { "Ch22_Eyelashes",     ("Ch22_1002_Diffuse", "Ch22_1002_Normal") },
            { "Ch22_Pants",         ("Ch22_1001_Diffuse", "Ch22_1001_Normal") },
            { "Ch22_Shirt",         ("Ch22_1001_Diffuse", "Ch22_1001_Normal") },
            { "Ch22_Sneakers",      ("Ch22_1001_Diffuse", "Ch22_1001_Normal") },
        };

        foreach (var kv in matMap)
        {
            var matPath = $"{matDir}/{kv.Key}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) { Debug.LogWarning($"[Lab5] Material not found: {matPath}"); continue; }

            var diffTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{matDir}/{kv.Value.diff}.png");
            var normTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{matDir}/{kv.Value.norm}.png");

            if (diffTex != null)
            {
                mat.SetTexture("_BaseMap", diffTex);
                mat.SetTexture("_MainTex", diffTex);
            }
            if (normTex != null)
            {
                // Mark as normal map
                var ti = AssetImporter.GetAtPath($"{matDir}/{kv.Value.norm}.png") as TextureImporter;
                if (ti != null && ti.textureType != TextureImporterType.NormalMap)
                {
                    ti.textureType = TextureImporterType.NormalMap;
                    ti.SaveAndReimport();
                }
                mat.SetTexture("_BumpMap", normTex);
                mat.EnableKeyword("_NORMALMAP");
            }

            EditorUtility.SetDirty(mat);
            Debug.Log($"[Lab5] Fixed material: {kv.Key}");
        }

        // Also assign materials to SkinnedMeshRenderers in scene
        AssignMaterialsToPlayer();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Lab5] Material textures fixed!");
    }

    static void AssignMaterialsToPlayer()
    {
        string matDir = "Assets/Models/Mixamo/Materials";
        var playerGO  = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null) return;

        var renderers = playerGO.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in renderers)
        {
            string name = smr.gameObject.name; // e.g. Ch22_Body, Ch22_Hair...
            string matName = name.Replace("Ch22_", "Ch22_").ToLower().Contains("hair") ? "Ch22_hair"
                           : name.ToLower().Contains("eye") ? "Ch22_hair"
                           : "Ch22_body";

            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{matDir}/{matName}.mat");
            if (mat != null)
            {
                smr.sharedMaterial = mat;
                Debug.Log($"[Lab5] Assigned {matName} → {smr.gameObject.name}");
            }
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    static void ExtractMaterials()
    {
        string matFolder = "Assets/Models/Mixamo/Materials";
        if (!AssetDatabase.IsValidFolder(matFolder))
            AssetDatabase.CreateFolder("Assets/Models/Mixamo", "Materials");

        foreach (var file in FbxFiles)
        {
            var path = $"{ModelsPath}/{file}";
            var imp  = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) continue;

            // Set material import mode to import embedded
            imp.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            imp.materialLocation   = ModelImporterMaterialLocation.External;

            // Extract textures first
            imp.ExtractTextures(matFolder);

            // Remap existing embedded materials to external
            var externals = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in externals)
            {
                if (!(asset is Material mat)) continue;
                var matPath = $"{matFolder}/{mat.name}.mat";
                if (!System.IO.File.Exists(
                        System.IO.Path.GetFullPath(
                            System.IO.Path.Combine(Application.dataPath, "..", matPath))))
                {
                    AssetDatabase.ExtractAsset(asset, matPath);
                }
            }
            imp.SaveAndReimport();
        }
    }

    [MenuItem("Tools/Lab 5/2 - Create Animation Assets")]
    static void Step2_CreateAnims()
    {
        EnsureFolders();
        CreateCoinController();
        CreatePlayerController();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Lab5] Step 2 done — AnimationClips + Controllers created.");
    }

    [MenuItem("Tools/Lab 5/3 - Setup Player in Scene")]
    static void Step3_SetupScene()
    {
        SetupScenePlayer();
        SetupCoinPrefabs();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[Lab5] Step 3 done — Player & Coins configured in scene.");
    }

    [MenuItem("Tools/Lab 5/Run All Steps")]
    static void RunAll()
    {
        Step1_ImportFbx();
        Step2_CreateAnims();
        Step3_SetupScene();
    }

    [MenuItem("Tools/Lab 5/Fix All (reassign controllers)")]
    static void FixAll()
    {
        Step2_CreateAnims();  // recreate anim assets with correct paths
        Step3_SetupScene();   // reassign to scene objects
        Debug.Log("[Lab5] FixAll done!");
    }

    // ─── STEP 1 ───────────────────────────────

    static void EnsureFolders()
    {
        string[] paths = {
            "Assets/Models",           "Assets/Models/Mixamo",
            "Assets/Animations",       "Assets/Animations/Coin",
            "Assets/Animations/Player","Assets/AnimatorControllers"
        };
        foreach (var p in paths)
        {
            if (!AssetDatabase.IsValidFolder(p))
            {
                var parts = p.Split('/');
                var parent = string.Join("/", parts.Take(parts.Length - 1));
                AssetDatabase.CreateFolder(parent, parts.Last());
            }
        }
    }

    static void CopyFbxFiles()
    {
        foreach (var file in FbxFiles)
        {
            var src = Path.Combine(Downloads, file);
            var dst = Path.GetFullPath(Path.Combine(Application.dataPath, "../", ModelsPath, file));
            if (File.Exists(src) && !File.Exists(dst))
            {
                File.Copy(src, dst);
                Debug.Log($"[Lab5] Copied {file}");
            }
        }
    }

    static void ConfigureImporters()
    {
        foreach (var file in FbxFiles)
        {
            var path = $"{ModelsPath}/{file}";
            var imp  = AssetImporter.GetAtPath(path) as ModelImporter;
            if (imp == null) { Debug.LogWarning($"[Lab5] Importer not found: {path}"); continue; }

            imp.animationType  = ModelImporterAnimationType.Human;
            imp.avatarSetup    = ModelImporterAvatarSetup.CreateFromThisModel;

            bool looped = file.Contains("Walk") || file.Contains("Run");
            if (imp.defaultClipAnimations.Length > 0)
            {
                var clips = imp.defaultClipAnimations;
                clips[0].loop       = looped;
                clips[0].loopTime   = looped;
                clips[0].lockRootRotation = true;
                clips[0].lockRootHeightY  = true;
                imp.clipAnimations  = clips;
            }

            imp.SaveAndReimport();
        }
    }

    // ─── STEP 2 ───────────────────────────────

    static void CreateCoinController()
    {
        // ── CoinRotate: loops Y 0→360 over 3 s ──
        var rotClip        = new AnimationClip { name = "CoinRotate" };
        var rotCurve       = AnimationCurve.Linear(0f, 0f, 3f, 360f);
        rotClip.SetCurve("", typeof(Transform), "localEulerAngles.y", rotCurve);
        var rotSettings    = AnimationUtility.GetAnimationClipSettings(rotClip);
        rotSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(rotClip, rotSettings);
        Save(rotClip, $"{AnimCoin}/CoinRotate.anim");

        // ── CoinPickup: bounce up + scale out ──
        var pickClip = new AnimationClip { name = "CoinPickup" };
        pickClip.SetCurve("", typeof(Transform), "localPosition.y",
            new AnimationCurve(
                new Keyframe(0f,  0f,   0f,  8f),
                new Keyframe(0.2f, 0.8f, 0f,  0f),
                new Keyframe(0.45f, 0f, -4f,  0f)));
        var scaleOut = new AnimationCurve(
                new Keyframe(0f,   1f),
                new Keyframe(0.15f, 1.5f),
                new Keyframe(0.45f, 0f));
        pickClip.SetCurve("", typeof(Transform), "localScale.x", scaleOut);
        pickClip.SetCurve("", typeof(Transform), "localScale.y", scaleOut);
        pickClip.SetCurve("", typeof(Transform), "localScale.z", scaleOut);
        Save(pickClip, $"{AnimCoin}/CoinPickup.anim");

        // ── Controller ──
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(
            $"{CtrlPath}/CoinController.controller");
        ctrl.AddParameter("Pickup", AnimatorControllerParameterType.Trigger);
        var sm  = ctrl.layers[0].stateMachine;

        var rotState  = sm.AddState("Rotate");  rotState.motion  = rotClip;
        var pickState = sm.AddState("Pickup");  pickState.motion = pickClip;
        sm.defaultState = rotState;

        var t = rotState.AddTransition(pickState);
        t.AddCondition(AnimatorConditionMode.If, 0, "Pickup");
        t.hasExitTime = false;
        t.duration    = 0f;

        AssetDatabase.SaveAssets();
    }

    static void CreatePlayerController()
    {
        // ── PlayerHit: color flash red on Ch22_Body ──
        // Animator is ON "Model", so child paths are relative: "Ch22_Body", "Ch22_Hair" etc.
        var hitClip = new AnimationClip { name = "PlayerHit" };
        var constOne = AnimationCurve.Constant(0f, 1f, 1f);   // red stays 1
        var greenBlue = new AnimationCurve();                  // flicker: 1→0→1...
        for (int i = 0; i <= 8; i++)
        {
            int idx = greenBlue.AddKey(new Keyframe(i * 0.12f, i % 2 == 0 ? 1f : 0f));
            AnimationUtility.SetKeyLeftTangentMode(greenBlue,  idx, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(greenBlue, idx, AnimationUtility.TangentMode.Constant);
        }
        // Animate material _BaseColor: red=1 always, green+blue flicker → white↔red
        hitClip.SetCurve("Ch22_Body", typeof(SkinnedMeshRenderer), "material._BaseColor.r", constOne);
        hitClip.SetCurve("Ch22_Body", typeof(SkinnedMeshRenderer), "material._BaseColor.g", greenBlue);
        hitClip.SetCurve("Ch22_Body", typeof(SkinnedMeshRenderer), "material._BaseColor.b", greenBlue);
        hitClip.SetCurve("Ch22_Body", typeof(SkinnedMeshRenderer), "material._BaseColor.a", AnimationCurve.Constant(0f, 1f, 1f));
        Save(hitClip, $"{AnimPlayer}/PlayerHit.anim");

        // ── Celebrate: jump up-down loop ──
        // path "" = the Model object itself (Animator is on Model)
        var celebClip = new AnimationClip { name = "Celebrate" };
        var bounceY = new AnimationCurve(
            new Keyframe(0f,    0f,  0f, 6f),
            new Keyframe(0.25f, 0.5f, 0f, 0f),
            new Keyframe(0.5f,  0f, -6f, 0f));
        celebClip.SetCurve("", typeof(Transform), "localPosition.y", bounceY);
        var cs = AnimationUtility.GetAnimationClipSettings(celebClip);
        cs.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(celebClip, cs);
        Save(celebClip, $"{AnimPlayer}/Celebrate.anim");

        // ── Load Mixamo clips ──
        var walkClip = GetClip("Walking");
        var runClip  = GetClip("Running");
        var jumpClip = GetClip("Jumping");
        var dieClip  = GetClip("Death");

        // ── Controller ──
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(
            $"{CtrlPath}/PlayerController.controller");
        ctrl.AddParameter("Speed",         AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsGrounded",    AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsDead",        AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsCelebrating", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Hit",           AnimatorControllerParameterType.Trigger);

        var sm   = ctrl.layers[0].stateMachine;
        var idle = sm.AddState("Idle");
        var walk = sm.AddState("Walk");      walk.motion = walkClip;
        var run  = sm.AddState("Run");       run.motion  = runClip;
        var jump = sm.AddState("Jump");      jump.motion = jumpClip;
        var die  = sm.AddState("Death");     die.motion  = dieClip;
        var celeb= sm.AddState("Celebrate"); celeb.motion = celebClip;
        var hit  = sm.AddState("Hit");       hit.motion  = hitClip;
        sm.defaultState = idle;

        // AnyState → Death / Celebrate
        AnyTo(sm, die,   "IsDead",        AnimatorConditionMode.If);
        AnyTo(sm, celeb, "IsCelebrating", AnimatorConditionMode.If);
        var anyHit = sm.AddAnyStateTransition(hit);
        anyHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        anyHit.hasExitTime = false; anyHit.duration = 0f; anyHit.canTransitionToSelf = false;

        // Hit → Idle
        var hExit = hit.AddTransition(idle);
        hExit.hasExitTime = true; hExit.exitTime = 1f; hExit.duration = 0f;

        // Idle ↔ Walk ↔ Run
        FloatTrans(idle, walk, "Speed", AnimatorConditionMode.Greater, 0.5f);
        FloatTrans(walk, idle, "Speed", AnimatorConditionMode.Less,    0.5f);
        FloatTrans(walk, run,  "Speed", AnimatorConditionMode.Greater, 7f);
        FloatTrans(run,  walk, "Speed", AnimatorConditionMode.Less,    7f);

        // → Jump from idle/walk/run
        BoolTrans(idle, jump, "IsGrounded", false);
        BoolTrans(walk, jump, "IsGrounded", false);
        BoolTrans(run,  jump, "IsGrounded", false);

        // Jump → Idle (land)
        var jLand = jump.AddTransition(idle);
        jLand.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        jLand.hasExitTime = true; jLand.exitTime = 0.5f; jLand.duration = 0.1f;

        AssetDatabase.SaveAssets();
    }

    // ─── STEP 3 ───────────────────────────────

    static void SetupScenePlayer()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null) { Debug.LogWarning("[Lab5] No 'Player' tag found in scene."); return; }

        var bridge = playerGO.GetComponent<PlayerAnimatorBridge>()
                     ?? playerGO.AddComponent<PlayerAnimatorBridge>();

        // Load assets
        var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{CtrlPath}/PlayerController.controller");
        Avatar avatar = null;
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath($"{ModelsPath}/Ch22_nonPBR@Walking Left Turn.fbx"))
            if (a is Avatar av) { avatar = av; break; }

        // Store in serialized fields so they survive domain reloads
        bridge.controllerAsset = ctrl;
        bridge.avatarAsset     = avatar;
        EditorUtility.SetDirty(bridge);

        var modelTf = playerGO.transform.Find("Model");
        if (modelTf == null)
        {
            var walkFbx = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelsPath}/Ch22_nonPBR@Walking Left Turn.fbx");
            if (walkFbx == null) { Debug.LogWarning("[Lab5] Walking FBX not found — run Step 1 first."); return; }

            var model = (GameObject)PrefabUtility.InstantiatePrefab(walkFbx, playerGO.transform);
            model.name                    = "Model";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale    = Vector3.one;
            modelTf = model.transform;
        }

        // Always (re)assign Animator on Model
        var animator = modelTf.GetComponent<Animator>() ?? modelTf.gameObject.AddComponent<Animator>();
        if (ctrl    != null) animator.runtimeAnimatorController = ctrl;
        if (avatar  != null) animator.avatar = avatar;
        EditorUtility.SetDirty(animator);

        // Disable capsule MeshRenderer
        var mr = playerGO.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        Debug.Log($"[Lab5] Player setup done. Controller={ctrl != null} Avatar={avatar != null}");
    }

    static void SetupCoinPrefabs()
    {
        var coinCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{CtrlPath}/CoinController.controller");
        if (coinCtrl == null) { Debug.LogWarning("[Lab5] CoinController not found — run Step 2 first."); return; }

        // Apply to all Coin GameObjects in the open scene
        var coins = GameObject.FindObjectsByType<Coin>(FindObjectsSortMode.None);
        foreach (var coin in coins)
        {
            var a = coin.GetComponent<Animator>();
            if (a == null) a = coin.gameObject.AddComponent<Animator>();
            a.runtimeAnimatorController = coinCtrl;
            Debug.Log($"[Lab5] Animator added to coin: {coin.gameObject.name}");
        }
    }

    // ─── Helpers ──────────────────────────────

    static AnimationClip GetClip(string keyword)
    {
        foreach (var file in FbxFiles)
        {
            if (!file.ToLower().Contains(keyword.ToLower())) continue;
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath($"{ModelsPath}/{file}"))
                if (a is AnimationClip ac && !ac.name.StartsWith("__preview__"))
                    return ac;
        }
        Debug.LogWarning($"[Lab5] Clip not found for keyword: {keyword}");
        return null;
    }

    static void Save(Object asset, string path)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(asset, path);
    }

    static void AnyTo(AnimatorStateMachine sm, AnimatorState to, string param, AnimatorConditionMode mode)
    {
        var t = sm.AddAnyStateTransition(to);
        t.AddCondition(mode, 0, param);
        t.hasExitTime = false; t.duration = 0.05f; t.canTransitionToSelf = false;
    }

    static void FloatTrans(AnimatorState from, AnimatorState to, string param, AnimatorConditionMode mode, float val)
    {
        var t = from.AddTransition(to);
        t.AddCondition(mode, val, param);
        t.hasExitTime = false; t.duration = 0.1f;
    }

    static void BoolTrans(AnimatorState from, AnimatorState to, string param, bool isTrue)
    {
        var t = from.AddTransition(to);
        t.AddCondition(isTrue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
        t.hasExitTime = false; t.duration = 0.05f;
    }
}
