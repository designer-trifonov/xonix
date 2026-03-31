using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HippoGame.Editor
{
    public static class YG2SetupHelper
    {
        private const string InterAdvPrefab  = "Assets/PluginYourGames/Modules/InterstitialAdv/Prefabs/Ad Notification.prefab";
        private const string TimerAdvPrefab  = "Assets/PluginYourGames/Modules/InterstitialAdv/Prefabs/Timer Before Ads.prefab";
        private const string RewardedPrefab  = "Assets/PluginYourGames/Modules/RewardedAdv/Prefabs/Lock Timer.prefab";
        private const string YandexTemplate  = "YandexGames";

        [MenuItem("Tools/HippoGame/Setup YG2 в сцене", false, 0)]
        public static void SetupScene()
        {
            ApplyPlayerSettings();
            AddPrefabsToScene();
            EditorSceneManager.SaveOpenScenes();

            // Открываем окно настроек плагина
            var windowType = System.Type.GetType("YG.EditorScr.InfoYGEditorWindow, Assembly-CSharp-Editor");
            if (windowType != null)
                EditorWindow.GetWindow(windowType).Show();
            else
                Debug.LogWarning("[YG2Setup] Окно настроек не найдено — открой вручную: Tools → YG2 → Settings");

            Debug.Log("[YG2Setup] Готово! Player Settings настроены, префабы добавлены в сцену.");
        }

        // ── Player Settings ────────────────────────────────────────────────────

        static void ApplyPlayerSettings()
        {
            // Build Target → WebGL
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                Debug.Log("[YG2Setup] Build Target переключён на WebGL");
            }

            // WebGL Template → YandexGames
            PlayerSettings.WebGL.template = $"PROJECT:{YandexTemplate}";
            Debug.Log($"[YG2Setup] WebGL Template → {YandexTemplate}");

            // Отключаем компрессию — Яндекс не поддерживает Brotli/Gzip без сервера
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

            // Имя продукта (оставляем, просто логируем)
            Debug.Log($"[YG2Setup] Product Name: {PlayerSettings.productName}");

            AssetDatabase.SaveAssets();
        }

        // ── Префабы в сцену ────────────────────────────────────────────────────

        static void AddPrefabsToScene()
        {
            Scene scene = SceneManager.GetActiveScene();

            TryAddPrefab(InterAdvPrefab, "Ad Notification", scene);
            TryAddPrefab(TimerAdvPrefab, "Timer Before Ads", scene);
            TryAddPrefab(RewardedPrefab, "Lock Timer", scene);
        }

        static void TryAddPrefab(string assetPath, string goName, Scene scene)
        {
            // Не добавляем дважды
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == goName)
                {
                    Debug.Log($"[YG2Setup] '{goName}' уже есть в сцене — пропускаем.");
                    return;
                }
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[YG2Setup] Префаб не найден: {assetPath}");
                return;
            }

            PrefabUtility.InstantiatePrefab(prefab);
            Debug.Log($"[YG2Setup] Добавлен: {goName}");
        }
    }
}
