using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using TMPro;
using HippoGame.Core;
using HippoGame.Zone;
using HippoGame.Grid;
using HippoGame.Ball;
using HippoGame.UI;
using HippoGame.Hippo;
using HippoGame.Trail;

namespace HippoGame.Editor
{
    public static class SceneBuilder
    {
        [MenuItem("HippoGame/Build Scene")]
        public static void BuildScene()
        {
            ClearExisting();

            LevelConfig config = GetOrCreateLevelConfig();

            GameObject controllers = new GameObject("Controllers");

            GameZone            zone       = CreateGameZone(controllers);
            HippoController     hippo      = CreateHippo(controllers);
            GameGrid            grid       = CreateGameGrid(controllers);
            HippoGridInteractor interactor = CreateGridInteractor(controllers);
            TrailLineRenderer   trail      = CreateTrailVisualizer(controllers);
            BallSpawner         spawner    = CreateBallSpawner(controllers);

            UIControllers        ui       = SetupUIControllers(controllers);
            GameOverUIController gameOver = CreateGameOverPanel(controllers);

            CreateBootstrap(zone, hippo, grid, interactor, trail, spawner, ui, gameOver, config);
            SetupCamera();

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[SceneBuilder] Сцена собрана.");
        }

        private struct UIControllers
        {
            public LivesUIController       Lives;
            public ScoreUIController       Score;
            public LevelUIController       Level;
            public FillPercentUIController Percent;
        }

        // ── Очистка ─────────────────────────────────────────────────────────
        private static void ClearExisting()
        {
            DestroyIfExists<GameZone>();
            DestroyIfExists<HippoController>();
            DestroyIfExists<GameGrid>();
            DestroyIfExists<HippoGridInteractor>();
            DestroyIfExists<TrailLineRenderer>();
            DestroyIfExists<BallSpawner>();
            DestroyIfExists<Bootstrap>();

            RemoveComponentIfExists<LivesUIController>();
            RemoveComponentIfExists<ScoreUIController>();
            RemoveComponentIfExists<LevelUIController>();
            RemoveComponentIfExists<FillPercentUIController>();

            // GameOver panel
            GameObject existingGO = GameObject.Find("GameOverPanel");
            if (existingGO != null) Object.DestroyImmediate(existingGO);

            GameObject existing = GameObject.Find("Controllers");
            if (existing != null) Object.DestroyImmediate(existing);
        }

        private static void DestroyIfExists<T>() where T : Component
        {
            T obj = Object.FindObjectOfType<T>();
            if (obj != null) Object.DestroyImmediate(obj.gameObject);
        }

        private static void RemoveComponentIfExists<T>() where T : Component
        {
            T obj = Object.FindObjectOfType<T>();
            if (obj != null) Object.DestroyImmediate(obj);
        }

        // ── Создание объектов ────────────────────────────────────────────────
        private static GameObject CreateChild(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            return go;
        }

        private static GameZone CreateGameZone(GameObject parent)
        {
            GameObject go = CreateChild("GameZoneController", parent);
            go.transform.position = Vector3.zero;
            return go.AddComponent<GameZone>();
        }

        private static HippoController CreateHippo(GameObject parent)
        {
            GameObject go = new GameObject("HippoController");
            go.transform.SetParent(parent.transform);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.color        = Color.red;
            sr.sortingOrder = 10;
            return go.AddComponent<HippoController>();
        }

        private static GameGrid CreateGameGrid(GameObject parent)
        {
            GameObject go = CreateChild("GameGridController", parent);
            go.transform.position = Vector3.zero;
            return go.AddComponent<GameGrid>();
        }

        private static HippoGridInteractor CreateGridInteractor(GameObject parent)
            => CreateChild("GridInteractorController", parent).AddComponent<HippoGridInteractor>();

        private static TrailLineRenderer CreateTrailVisualizer(GameObject parent)
        {
            GameObject go = CreateChild("TrailController", parent);
            go.AddComponent<LineRenderer>();
            return go.AddComponent<TrailLineRenderer>();
        }

        private static BallSpawner CreateBallSpawner(GameObject parent)
            => CreateChild("BallSpawnerController", parent).AddComponent<BallSpawner>();

        // ── UI контроллеры ───────────────────────────────────────────────────
        private static UIControllers SetupUIControllers(GameObject parent)
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
                Debug.LogWarning("[SceneBuilder] Canvas не найден — UI поля будут пустыми.");

            return new UIControllers
            {
                Lives   = CreateUIController<LivesUIController>(parent,       "LivesUIController",   canvas, "Live_Text"),
                Score   = CreateUIController<ScoreUIController>(parent,       "ScoreUIController",   canvas, "Score_Text"),
                Level   = CreateUIController<LevelUIController>(parent,       "LevelUIController",   canvas, "Level_Text"),
                Percent = CreateUIController<FillPercentUIController>(parent,  "PercentUIController", canvas, "Percent_Text")
            };
        }

        private static T CreateUIController<T>(GameObject parent, string goName, Canvas canvas, string textObjectName) where T : Component
        {
            GameObject go = CreateChild(goName, parent);
            T controller = go.AddComponent<T>();

            if (canvas != null)
            {
                TMP_Text tmp = FindTMPInChildren(canvas.transform, textObjectName);
                if (tmp != null)
                {
                    SerializedObject so = new SerializedObject(controller);
                    SerializedProperty prop = so.FindProperty("_text");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = tmp;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            EditorUtility.SetDirty(controller);
            return controller;
        }

        // ── GameOver панель ──────────────────────────────────────────────────
        private static GameOverUIController CreateGameOverPanel(GameObject parent)
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[SceneBuilder] Canvas не найден — GameOver панель не создана.");
                return null;
            }

            // Корневая панель
            GameObject panelGO = new GameObject("GameOverPanel");
            panelGO.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin     = new Vector2(0.25f, 0.25f);
            panelRect.anchorMax     = new Vector2(0.75f, 0.75f);
            panelRect.offsetMin     = Vector2.zero;
            panelRect.offsetMax     = Vector2.zero;

            Image bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);

            GameOverUIController controller = panelGO.AddComponent<GameOverUIController>();

            // Заголовок
            TMP_Text title = CreateUIText(panelGO, "Title", "Вы проиграли", 36, new Vector2(0f, 60f));
            title.alignment = TextAlignmentOptions.Center;
            title.color     = Color.white;

            // Кнопка — реклама
            Button adBtn     = CreateButton(panelGO, "WatchAdButton",  "Продолжить за рекламу", new Vector2(0f, 0f));
            TMP_Text adText  = adBtn.GetComponentInChildren<TMP_Text>();

            // Кнопка — рестарт
            Button restartBtn = CreateButton(panelGO, "RestartButton", "Заново с 1-го уровня", new Vector2(0f, -50f));

            // Wiring
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("_panel").objectReferenceValue         = panelGO;
            so.FindProperty("_watchAdButton").objectReferenceValue = adBtn;
            so.FindProperty("_restartButton").objectReferenceValue = restartBtn;
            so.FindProperty("_adButtonText").objectReferenceValue  = adText;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static TMP_Text CreateUIText(GameObject parent, string name, string text, int fontSize, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = new Vector2(350f, 50f);

            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text     = text;
            tmp.fontSize = fontSize;
            return tmp;
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = new Vector2(250f, 40f);

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 1f);

            Button btn = go.AddComponent<Button>();

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);

            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TMP_Text tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            return btn;
        }

        // ── LevelConfig ──────────────────────────────────────────────────────
        private static LevelConfig GetOrCreateLevelConfig()
        {
            // Ищем существующий asset
            string[] guids = AssetDatabase.FindAssets("t:LevelConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                LevelConfig existing = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
                Debug.Log($"[SceneBuilder] LevelConfig найден: {path}");
                return existing;
            }

            // Создаём новый
            string dir = "Assets/Resources";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string assetPath = $"{dir}/LevelConfig.asset";

            LevelConfig config = ScriptableObject.CreateInstance<LevelConfig>();
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SceneBuilder] LevelConfig создан: {assetPath}");
            return config;
        }

        // ── Bootstrap ────────────────────────────────────────────────────────
        private static void CreateBootstrap(GameZone zone,
            HippoController hippo, GameGrid grid, HippoGridInteractor interactor,
            TrailLineRenderer trail, BallSpawner spawner, UIControllers ui,
            GameOverUIController gameOver, LevelConfig config)
        {
            GameObject go        = new GameObject("Bootstrap");
            Bootstrap  bootstrap = go.AddComponent<Bootstrap>();

            SerializedObject so = new SerializedObject(bootstrap);
            so.FindProperty("_levelConfig").objectReferenceValue         = config;
            so.FindProperty("_gameZone").objectReferenceValue            = zone;
            so.FindProperty("_hippoController").objectReferenceValue     = hippo;
            so.FindProperty("_gameGrid").objectReferenceValue            = grid;
            so.FindProperty("_hippoGridInteractor").objectReferenceValue = interactor;
            so.FindProperty("_movementTrail").objectReferenceValue       = trail;
            so.FindProperty("_ballSpawner").objectReferenceValue         = spawner;
            so.FindProperty("_livesUI").objectReferenceValue             = ui.Lives;
            so.FindProperty("_scoreUI").objectReferenceValue             = ui.Score;
            so.FindProperty("_levelUI").objectReferenceValue             = ui.Level;
            so.FindProperty("_percentUI").objectReferenceValue           = ui.Percent;
            so.FindProperty("_gameOverUI").objectReferenceValue          = gameOver;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(bootstrap);
        }

        // ── Камера ───────────────────────────────────────────────────────────
        private static void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }

            cam.orthographic       = true;
            cam.orthographicSize   = 4.5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.backgroundColor    = new Color(0.1f, 0.1f, 0.1f);
            cam.clearFlags         = CameraClearFlags.SolidColor;

            Component ppc = cam.GetComponent("PixelPerfectCamera");
            if (ppc != null) Object.DestroyImmediate(ppc);

            EditorUtility.SetDirty(cam);
        }

        private static TMP_Text FindTMPInChildren(Transform root, string name)
        {
            foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>(true))
                if (t.gameObject.name == name)
                    return t;

            Debug.LogWarning($"[SceneBuilder] TMP_Text '{name}' не найден в Canvas!");
            return null;
        }
    }
}
