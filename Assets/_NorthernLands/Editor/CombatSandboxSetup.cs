using System.Collections.Generic;
using System.Linq;
using NorthernLands.AI;
using NorthernLands.CameraSystem;
using NorthernLands.Combat;
using NorthernLands.Player.Input;
using NorthernLands.Player.Movement;
using NorthernLands.UI.HUD;
using NorthernLands.UI.MobileControls;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NorthernLands.Editor
{
    public static class CombatSandboxSetup
    {
        private const string ScenePath = "Assets/_NorthernLands/Scenes/90_CombatSandbox.unity";

        [MenuItem("Tools/Northern Lands/4. Create Combat Sandbox")]
        public static void CreateCombatSandbox()
        {
            if (System.IO.File.Exists(ScenePath)
                && !EditorUtility.DisplayDialog(
                    "Северные Земли XIV",
                    "Сцена 90_CombatSandbox уже существует. Пересоздать её?",
                    "Пересоздать",
                    "Отмена"))
                return;

            System.IO.Directory.CreateDirectory("Assets/_NorthernLands/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateEnvironment();
            var camera = CreateCamera();
            var player = CreatePlayer(camera.transform, out var input, out var health, out var dash);
            camera.GetComponent<SimpleThirdPersonCamera>().Configure(player.transform, input);

            CreateEnemy(new Vector3(0f, 1f, 7f), player.transform, 85f);
            CreateEnemy(new Vector3(-5f, 1f, 10f), player.transform, 110f);
            CreateEnemy(new Vector3(6f, 1f, 12f), player.transform, 130f);
            CreateMobileHud(input, health, dash);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            Selection.activeGameObject = player;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Northern Lands: combat sandbox created. Open 90_CombatSandbox and press Play.");
        }

        private static void CreateEnvironment()
        {
            var root = new GameObject("COMBAT_SANDBOX_ROOT");

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.position = new Vector3(0f, -0.5f, 5f);
            ground.transform.localScale = new Vector3(32f, 1f, 32f);

            var lightObject = new GameObject("Sun", typeof(Light));
            lightObject.transform.SetParent(root.transform);
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
        }

        private static GameObject CreateCamera()
        {
            var cameraObject = new GameObject(
                "Main Camera",
                typeof(Camera),
                typeof(AudioListener),
                typeof(SimpleThirdPersonCamera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 4f, -6f);
            return cameraObject;
        }

        private static GameObject CreatePlayer(
            Transform movementCamera,
            out PlayerInputRouter input,
            out HealthComponent health,
            out DashChargeController dash)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_FallbackCapsule";
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(player.GetComponent<Collider>());

            player.AddComponent<CharacterController>();
            input = player.AddComponent<PlayerInputRouter>();
            health = player.AddComponent<HealthComponent>();
            health.Configure(120f, 10f);

            var attackOrigin = new GameObject("AttackOrigin").transform;
            attackOrigin.SetParent(player.transform);
            attackOrigin.localPosition = new Vector3(0f, 0.55f, 1.15f);

            var motor = player.AddComponent<SimpleThirdPersonMotor>();
            motor.Configure(input, movementCamera, health);

            var combat = player.AddComponent<PlayerCombatController>();
            combat.Configure(input, health, attackOrigin);

            dash = player.AddComponent<DashChargeController>();
            dash.Configure(input, health);

            var respawn = player.AddComponent<PlayerRespawnController>();
            respawn.Configure(player.transform.position);
            return player;
        }

        private static void CreateEnemy(Vector3 position, Transform target, float healthValue)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = $"TrainingEnemy_{Mathf.RoundToInt(healthValue)}HP";
            enemy.transform.position = position;
            Object.DestroyImmediate(enemy.GetComponent<Collider>());

            enemy.AddComponent<CharacterController>();
            var health = enemy.AddComponent<HealthComponent>();
            health.Configure(healthValue, 5f);
            var controller = enemy.AddComponent<TrainingEnemyController>();
            controller.Configure(target);
        }

        private static void CreateMobileHud(
            PlayerInputRouter input,
            HealthComponent health,
            DashChargeController dash)
        {
            var canvasObject = new GameObject(
                "MobileHUD",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(eventSystem, SceneManager.GetActiveScene());

            CreateLookSurface(canvasObject.transform, input);
            CreateJoystick(canvasObject.transform, input);
            CreateActionButton(canvasObject.transform, input, "АТАКА", new Vector2(-150f, 165f), 150f, VirtualActionButton.Action.LightAttack);
            CreateActionButton(canvasObject.transform, input, "СИЛА", new Vector2(-315f, 235f), 125f, VirtualActionButton.Action.HeavyAttack);
            CreateActionButton(canvasObject.transform, input, "БЛОК", new Vector2(-480f, 155f), 120f, VirtualActionButton.Action.Block);
            CreateActionButton(canvasObject.transform, input, "РЫВОК", new Vector2(-305f, 70f), 120f, VirtualActionButton.Action.Dash);

            var status = CreateText(canvasObject.transform, "CombatStatus", 30, TextAnchor.UpperLeft);
            var statusRect = status.rectTransform;
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(0f, 1f);
            statusRect.pivot = new Vector2(0f, 1f);
            statusRect.anchoredPosition = new Vector2(30f, -30f);
            statusRect.sizeDelta = new Vector2(1000f, 180f);
            status.color = Color.white;
            status.horizontalOverflow = HorizontalWrapMode.Wrap;

            var hud = canvasObject.AddComponent<CombatSandboxHud>();
            hud.Configure(health, dash, status);
        }

        private static void CreateLookSurface(Transform parent, PlayerInputRouter input)
        {
            var surface = CreateRect(parent, "CameraLookSurface");
            surface.anchorMin = new Vector2(0.38f, 0f);
            surface.anchorMax = new Vector2(1f, 1f);
            surface.offsetMin = Vector2.zero;
            surface.offsetMax = Vector2.zero;
            var image = surface.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.001f);
            image.raycastTarget = true;
            var look = surface.gameObject.AddComponent<VirtualLookSurface>();
            look.Configure(input);
        }

        private static void CreateJoystick(Transform parent, PlayerInputRouter input)
        {
            var background = CreateRect(parent, "MovementJoystick");
            background.anchorMin = Vector2.zero;
            background.anchorMax = Vector2.zero;
            background.pivot = new Vector2(0.5f, 0.5f);
            background.anchoredPosition = new Vector2(220f, 210f);
            background.sizeDelta = new Vector2(280f, 280f);
            var backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.12f, 0.18f, 0.55f);

            var handle = CreateRect(background, "Handle");
            handle.anchorMin = new Vector2(0.5f, 0.5f);
            handle.anchorMax = new Vector2(0.5f, 0.5f);
            handle.pivot = new Vector2(0.5f, 0.5f);
            handle.anchoredPosition = Vector2.zero;
            handle.sizeDelta = new Vector2(120f, 120f);
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = new Color(0.35f, 0.75f, 1f, 0.85f);
            handleImage.raycastTarget = false;

            var joystick = background.gameObject.AddComponent<VirtualJoystick>();
            joystick.Configure(input, background, handle);
        }

        private static void CreateActionButton(
            Transform parent,
            PlayerInputRouter input,
            string label,
            Vector2 anchoredPosition,
            float size,
            VirtualActionButton.Action action)
        {
            var buttonRect = CreateRect(parent, $"Button_{label}");
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = new Vector2(size, size);
            var image = buttonRect.gameObject.AddComponent<Image>();
            image.color = action == VirtualActionButton.Action.LightAttack
                ? new Color(0.8f, 0.18f, 0.12f, 0.82f)
                : new Color(0.12f, 0.22f, 0.34f, 0.82f);

            var actionButton = buttonRect.gameObject.AddComponent<VirtualActionButton>();
            actionButton.Configure(input, action);

            var text = CreateText(buttonRect, "Label", Mathf.RoundToInt(size * 0.18f), TextAnchor.MiddleCenter);
            text.text = label;
            text.color = Color.white;
            text.raycastTarget = false;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static Text CreateText(Transform parent, string name, int fontSize, TextAnchor alignment)
        {
            var rect = CreateRect(parent, name);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            return text;
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(scene => scene.path != path))
                scenes.Add(new EditorBuildSettingsScene(path, true));
            else
                scenes = scenes.Select(scene => scene.path == path
                    ? new EditorBuildSettingsScene(path, true)
                    : scene).ToList();

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
