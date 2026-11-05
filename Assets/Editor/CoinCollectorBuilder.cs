using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Herramienta de editor que monta el proyecto Coin Collector 3D:
/// tag Ground, materiales, prefab Coin, escenas Level1 y MenuPrincipal y Build Settings.
/// Menú: Tools > Coin Collector > Construir todo
/// </summary>
public static class CoinCollectorBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string PrefabsFolder = "Assets/Prefabs";
    private const string MaterialsFolder = "Assets/Materials";
    private const string Level1Path = ScenesFolder + "/Level1.unity";
    private const string MenuPath = ScenesFolder + "/MenuPrincipal.unity";
    private const string CoinPrefabPath = PrefabsFolder + "/Coin.prefab";

    [MenuItem("Tools/Coin Collector/Construir todo")]
    public static void BuildAll()
    {
        EnsureFolders();
        bool inputChanged = SetActiveInputHandlingBoth();
        EnsureTag("Ground");

        Material gold = CreateMaterial("Gold", new Color(1f, 0.78f, 0.1f), 0.8f, 0.7f);
        Material ground = CreateMaterial("Ground", new Color(0.35f, 0.55f, 0.35f), 0f, 0.2f);
        Material player = CreateMaterial("Player", new Color(0.15f, 0.4f, 0.9f), 0.1f, 0.6f);

        GameObject coinPrefab = CreateCoinPrefab(gold);

        BuildLevel1(ground, player, coinPrefab);
        BuildMenu();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuPath, true),
            new EditorBuildSettingsScene(Level1Path, true),
        };

        // La SampleScene de la plantilla ya no hace falta.
        if (File.Exists(ScenesFolder + "/SampleScene.unity"))
        {
            AssetDatabase.DeleteAsset(ScenesFolder + "/SampleScene.unity");
        }

        AssetDatabase.SaveAssets();

        if (!Application.isBatchMode)
        {
            EditorSceneManager.OpenScene(Level1Path);
        }

        Debug.Log("[CoinCollectorBuilder] Proyecto montado correctamente.");
        if (inputChanged && !Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Coin Collector",
                "Proyecto montado.\n\nSe ha cambiado Active Input Handling a 'Both'. " +
                "Reinicia Unity para que el movimiento con WASD funcione.", "OK");
        }
    }

    // ---------------------------------------------------------------- Ajustes

    private static void EnsureFolders()
    {
        foreach (string folder in new[] { ScenesFolder, PrefabsFolder, MaterialsFolder })
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            }
        }
    }

    /// <summary>Pone Active Input Handling = Both (2) para poder usar Input.GetAxis.</summary>
    private static bool SetActiveInputHandlingBoth()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (assets.Length == 0) return false;

        var so = new SerializedObject(assets[0]);
        SerializedProperty prop = so.FindProperty("activeInputHandler");
        if (prop == null || prop.intValue == 2) return false;

        prop.intValue = 2;
        so.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }

    private static void EnsureTag(string tag)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        var so = new SerializedObject(assets[0]);
        SerializedProperty tags = so.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ------------------------------------------------------------- Materiales

    private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
    {
        string path = $"{MaterialsFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.SetColor("_BaseColor", color);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Glossiness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // ----------------------------------------------------------------- Prefab

    private static GameObject CreateCoinPrefab(Material gold)
    {
        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
        coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // de canto, como una moneda
        coin.GetComponent<MeshRenderer>().sharedMaterial = gold;

        // Cambiamos el CapsuleCollider por un SphereCollider trigger.
        Object.DestroyImmediate(coin.GetComponent<CapsuleCollider>());
        SphereCollider col = coin.AddComponent<SphereCollider>();
        col.isTrigger = true;

        coin.AddComponent<Coin>();
        coin.AddComponent<Spinner>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(coin, CoinPrefabPath);
        Object.DestroyImmediate(coin);
        return prefab;
    }

    // ----------------------------------------------------------------- Level1

    private static void BuildLevel1(Material groundMat, Material playerMat, GameObject coinPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Cámara que ve todo el nivel
        Camera cam = Camera.main;
        cam.transform.SetPositionAndRotation(new Vector3(0f, 18f, -14f), Quaternion.Euler(50f, 0f, 0f));

        // Terra
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.tag = "Ground";
        ground.transform.localScale = new Vector3(2f, 1f, 2f); // 20 x 20
        ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

        // Parets invisibles (Is Trigger, Renderer desactivat)
        GameObject walls = new GameObject("Walls");
        CreateWall("Wall_N", new Vector3(0f, 1f, 10f), new Vector3(20f, 2f, 0.5f), walls.transform);
        CreateWall("Wall_S", new Vector3(0f, 1f, -10f), new Vector3(20f, 2f, 0.5f), walls.transform);
        CreateWall("Wall_E", new Vector3(10f, 1f, 0f), new Vector3(0.5f, 2f, 20f), walls.transform);
        CreateWall("Wall_W", new Vector3(-10f, 1f, 0f), new Vector3(0.5f, 2f, 20f), walls.transform);

        // KillZone
        GameObject killZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        killZone.name = "KillZone";
        killZone.transform.position = new Vector3(0f, -10f, 0f);
        killZone.transform.localScale = new Vector3(200f, 1f, 200f);
        killZone.GetComponent<MeshRenderer>().enabled = false;
        killZone.GetComponent<BoxCollider>().isTrigger = true;
        killZone.AddComponent<KillZone>();

        // Player
        GameObject playerGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        playerGo.name = "Player";
        playerGo.tag = "Player";
        playerGo.transform.position = new Vector3(0f, 0.5f, 0f);
        playerGo.GetComponent<MeshRenderer>().sharedMaterial = playerMat;
        Rigidbody rb = playerGo.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        playerGo.AddComponent<PlayerController>();

        // GameManager
        new GameObject("GameManager").AddComponent<GameManager>();

        // Monedes (8)
        GameObject coins = new GameObject("Coins");
        Vector3[] positions =
        {
            new Vector3(-6f, 0.6f, 6f), new Vector3(0f, 0.6f, 7f), new Vector3(6f, 0.6f, 6f),
            new Vector3(-7f, 0.6f, 0f), new Vector3(7f, 0.6f, 0f),
            new Vector3(-6f, 0.6f, -6f), new Vector3(0f, 0.6f, -7f), new Vector3(6f, 0.6f, -6f),
        };
        for (int i = 0; i < positions.Length; i++)
        {
            var coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab, coins.transform);
            coin.name = $"Coin_{i + 1}";
            coin.transform.position = positions[i];
        }

        EditorSceneManager.SaveScene(scene, Level1Path);
    }

    private static void CreateWall(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<MeshRenderer>().enabled = false;
        wall.GetComponent<BoxCollider>().isTrigger = true;
    }

    // ---------------------------------------------------------- MenuPrincipal

    private static void BuildMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Canvas
        GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        // EventSystem (con el módulo del Input System nuevo)
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Título
        GameObject title = new GameObject("Title", typeof(RectTransform), typeof(Text));
        title.transform.SetParent(canvasGo.transform, false);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0f, 200f);
        titleRect.sizeDelta = new Vector2(1200f, 160f);
        Text titleText = title.GetComponent<Text>();
        titleText.text = "Coin Collector 3D";
        titleText.font = font;
        titleText.fontSize = 110;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        // Botón Play
        GameObject buttonGo = new GameObject("PlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(canvasGo.transform, false);
        var buttonRect = buttonGo.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(400f, 120f);
        buttonGo.GetComponent<Image>().color = new Color(1f, 0.78f, 0.1f);

        GameObject label = new GameObject("Text", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(buttonGo.transform, false);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text labelText = label.GetComponent<Text>();
        labelText.text = "Play";
        labelText.font = font;
        labelText.fontSize = 60;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.black;

        // MenuManager + OnClick -> MainMenu.PlayGame
        MainMenu menu = new GameObject("MenuManager").AddComponent<MainMenu>();
        UnityEventTools.AddPersistentListener(buttonGo.GetComponent<Button>().onClick, menu.PlayGame);

        EditorSceneManager.SaveScene(scene, MenuPath);
    }
}
