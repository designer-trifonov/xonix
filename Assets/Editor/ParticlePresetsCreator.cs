using UnityEngine;
using UnityEditor;

namespace HippoGame.Editor
{
    /// Создаёт готовые префабы партикл-эффектов для игры одной кнопкой.
    /// Меню: Tools → HippoGame → Create Particle Presets
    public static class ParticlePresetsCreator
    {
        private const string SavePath = "Assets/Prefabs/FX";

        [MenuItem("Tools/HippoGame/Create Particle Presets")]
        public static void CreateAll()
        {
            System.IO.Directory.CreateDirectory(SavePath);

            CreateDockEffect();
            CreateBallHitEffect();

            AssetDatabase.Refresh();
            Debug.Log("[ParticlePresetsCreator] Оба эффекта созданы в " + SavePath);
        }

        // ── Стыковка гиппо со стеной ────────────────────────────────────────────
        private static void CreateDockEffect()
        {
            GameObject go = new GameObject("FX_Dock");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration        = 0.4f;
            main.loop            = false;
            main.startLifetime   = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed      = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSize       = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            main.startColor      = new ParticleSystem.MinMaxGradient(
                new Color(0.3f, 0.8f, 1f, 1f),   // голубой
                new Color(1f,   1f,   1f, 1f));   // белый
            main.maxParticles    = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

            var shape = ps.shape;
            shape.enabled   = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius    = 0.05f;

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = false;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            SetSpriteMaterial(ps);

            string path = $"{SavePath}/FX_Dock.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log("[ParticlePresetsCreator] FX_Dock → " + path);
        }

        // ── Попадание шара в трейл ──────────────────────────────────────────────
        private static void CreateBallHitEffect()
        {
            GameObject go = new GameObject("FX_BallHit");
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration        = 0.5f;
            main.loop            = false;
            main.startLifetime   = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            main.startSpeed      = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor      = new ParticleSystem.MinMaxGradient(
                new Color(1f,  0.3f, 0.1f, 1f),  // оранжево-красный
                new Color(1f,  0.9f, 0.1f, 1f));  // жёлтый
            main.maxParticles    = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.enabled   = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius    = 0.08f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.2f, 0f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            SetSpriteMaterial(ps);

            string path = $"{SavePath}/FX_BallHit.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log("[ParticlePresetsCreator] FX_BallHit → " + path);
        }

        private static void SetSpriteMaterial(ParticleSystem ps)
        {
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 10;
        }
    }
}
