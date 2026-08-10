using System;
using System.Linq;
using Unity.BossRoom.Gameplay.NorthernLands.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Object = UnityEngine.Object;

namespace NorthernLands.Editor
{
    /// <summary>
    /// Deterministically builds the first open-world campaign scene from the starter project's art.
    /// This runs before cloud export, so generated terrain data never has to be edited on a phone.
    /// </summary>
    public static class NorthernLandsRiverholmSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/NorthernLands.unity";

        const string k_GeneratedFolder = "Assets/NorthernLands/Generated";
        const string k_TerrainPath = k_GeneratedFolder + "/RiverholmTerrain.asset";
        const string k_TerrainTexturePath = k_GeneratedFolder + "/RiverholmGround.asset";
        const string k_TerrainLayerPath = k_GeneratedFolder + "/RiverholmGroundLayer.terrainlayer";
        const string k_CharacterPrefab = "Assets/Prefabs/CharGFX/CharacterGraphics/PlayerGraphics_Tank_Boy.prefab";
        const string k_AvatarPath = "Assets/Models/CharacterSet.fbx";
        const string k_ControllerPath = "Assets/Models/Animation Controllers/CharacterSetController.controller";

        [MenuItem("Northern Lands/Rebuild Riverholm Vertical Slice")]
        public static void Rebuild()
        {
            EnsureFolder("Assets/NorthernLands");
            EnsureFolder(k_GeneratedFolder);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureLighting();

            var terrain = BuildTerrain();
            var palette = BuildPalette();
            BuildRiver(terrain, palette.Water);
            BuildRoadNetwork(terrain, palette.Road);
            BuildRiverholm(terrain, palette);
            BuildForestAndLandmarks(terrain, palette);
            var player = BuildPlayer(terrain);
            BuildCamera(player.transform);
            BuildRuntimeSystems();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Northern Lands: rebuilt Riverholm terrain, town, forest, player and Android controls.");
        }

        static void ConfigureLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.3f, 0.39f, 0.5f);
            RenderSettings.ambientEquatorColor = new Color(0.16f, 0.2f, 0.23f);
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.08f, 0.09f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.32f, 0.4f, 0.47f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 105f;
            RenderSettings.fogEndDistance = 330f;

            var sun = new GameObject("Northern Sun", typeof(Light));
            sun.transform.rotation = Quaternion.Euler(43f, -28f, 0f);
            var light = sun.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.82f, 0.88f, 1f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
        }

        static Terrain BuildTerrain()
        {
            AssetDatabase.DeleteAsset(k_TerrainPath);
            var data = new TerrainData
            {
                heightmapResolution = 257,
                alphamapResolution = 256,
                baseMapResolution = 256,
                size = new Vector3(500f, 52f, 500f)
            };

            var resolution = data.heightmapResolution;
            var heights = new float[resolution, resolution];
            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var nx = x / (float)(resolution - 1);
                    var nz = z / (float)(resolution - 1);
                    var broad = Mathf.PerlinNoise(nx * 3.1f + 12.4f, nz * 3.1f + 7.8f) * 0.16f;
                    var detail = Mathf.PerlinNoise(nx * 11.7f + 4.2f, nz * 11.7f + 18.5f) * 0.035f;
                    var cityDistance = Vector2.Distance(new Vector2(nx, nz), new Vector2(0.5f, 0.46f));
                    var cityBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.11f, 0.24f, cityDistance));
                    var riverCenter = 0.67f + Mathf.Sin(nz * 8f) * 0.018f;
                    var riverDistance = Mathf.Abs(nx - riverCenter);
                    var riverCut = 1f - Mathf.SmoothStep(0.018f, 0.065f, riverDistance);
                    heights[z, x] = Mathf.Lerp(0.047f, 0.055f + broad + detail, cityBlend) - riverCut * 0.055f;
                }
            }

            data.SetHeights(0, 0, heights);
            data.terrainLayers = new[] { BuildTerrainLayer() };
            AssetDatabase.CreateAsset(data, k_TerrainPath);
            var terrainObject = Terrain.CreateTerrainGameObject(data);
            terrainObject.name = "Riverholm Highlands — 500m Explorable Terrain";
            terrainObject.transform.position = new Vector3(-250f, -2f, -250f);
            var terrain = terrainObject.GetComponent<Terrain>();
            terrain.heightmapPixelError = 12f;
            terrain.basemapDistance = 280f;
            terrain.drawInstanced = true;
            GameObjectUtility.SetStaticEditorFlags(terrainObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
            return terrain;
        }

        static Palette BuildPalette()
        {
            return new Palette
            {
                Stone = Material("NL_Stone", new Color(0.29f, 0.33f, 0.36f)),
                DarkStone = Material("NL_DarkStone", new Color(0.12f, 0.15f, 0.18f)),
                Timber = Material("NL_Timber", new Color(0.24f, 0.13f, 0.075f)),
                Roof = Material("NL_Roof", new Color(0.16f, 0.21f, 0.25f)),
                Pine = Material("NL_Pine", new Color(0.075f, 0.2f, 0.14f)),
                Water = Material("NL_Water", new Color(0.07f, 0.28f, 0.4f, 0.82f), true),
                Road = Material("NL_Road", new Color(0.25f, 0.22f, 0.18f)),
                Portal = Material("NL_Portal", new Color(0.23f, 0.68f, 0.95f), true)
            };
        }

        static void BuildRiver(Terrain terrain, Material water)
        {
            var river = GameObject.CreatePrimitive(PrimitiveType.Plane);
            river.name = "Glacial River";
            river.transform.SetPositionAndRotation(new Vector3(84f, 1.1f, 0f), Quaternion.identity);
            river.transform.localScale = new Vector3(3.3f, 1f, 50f);
            river.GetComponent<Renderer>().sharedMaterial = water;
            Object.DestroyImmediate(river.GetComponent<Collider>());
            GameObjectUtility.SetStaticEditorFlags(river, StaticEditorFlags.BatchingStatic);

            CreateBridge(new Vector3(82f, terrain.SampleHeight(new Vector3(82f, 0f, -24f)) + 0.7f, -24f));
        }

        static void BuildRoadNetwork(Terrain terrain, Material road)
        {
            CreateRoad(terrain, road, new Vector3(0f, 0f, -88f), new Vector3(0f, 0f, 80f), 9f);
            CreateRoad(terrain, road, new Vector3(-78f, 0f, -5f), new Vector3(72f, 0f, -5f), 8f);
            CreateRoad(terrain, road, new Vector3(35f, 0f, 42f), new Vector3(80f, 0f, -24f), 6f);
        }

        static void BuildRiverholm(Terrain terrain, Palette palette)
        {
            var town = new GameObject("Riverholm — Walled Northern Town");
            for (var i = -3; i <= 3; i++)
            {
                CreateWallBlock(town.transform, terrain, palette.Stone, new Vector3(i * 18f, 0f, 66f), new Vector3(16f, 5.5f, 3f));
                if (Mathf.Abs(i) > 1)
                {
                    CreateWallBlock(town.transform, terrain, palette.Stone, new Vector3(i * 18f, 0f, -63f), new Vector3(16f, 5.5f, 3f));
                }
            }

            for (var i = -3; i <= 3; i++)
            {
                CreateWallBlock(town.transform, terrain, palette.Stone, new Vector3(-64f, 0f, i * 18f), new Vector3(3f, 5.5f, 16f));
                CreateWallBlock(town.transform, terrain, palette.Stone, new Vector3(64f, 0f, i * 18f), new Vector3(3f, 5.5f, 16f));
            }

            var houses = new[]
            {
                new Vector3(-40f, 0f, 38f), new Vector3(-17f, 0f, 39f), new Vector3(18f, 0f, 41f),
                new Vector3(42f, 0f, 34f), new Vector3(-42f, 0f, 8f), new Vector3(39f, 0f, 7f),
                new Vector3(-38f, 0f, -30f), new Vector3(-13f, 0f, -35f), new Vector3(23f, 0f, -34f),
                new Vector3(45f, 0f, -30f)
            };

            for (var i = 0; i < houses.Length; i++)
            {
                CreateHouse(town.transform, terrain, palette, houses[i], 7f + i % 3 * 1.5f, i % 2 == 0 ? 0f : 90f);
            }

            CreateKeep(town.transform, terrain, palette, new Vector3(0f, 0f, 18f));
            CreateGate(town.transform, terrain, palette, new Vector3(0f, 0f, -64f));
        }

        static void BuildForestAndLandmarks(Terrain terrain, Palette palette)
        {
            var forest = new GameObject("Black Pine Forest");
            var random = new System.Random(14014);
            for (var i = 0; i < 95; i++)
            {
                var point = new Vector3((float)(random.NextDouble() * 430d - 215d), 0f, (float)(random.NextDouble() * 430d - 215d));
                if (Mathf.Abs(point.x) < 82f && Mathf.Abs(point.z) < 86f || Vector3.Distance(point, new Vector3(84f, 0f, point.z)) < 25f)
                {
                    continue;
                }

                CreatePine(forest.transform, terrain, palette, point, 0.75f + (float)random.NextDouble() * 0.65f);
            }

            CreatePortal(terrain, palette, new Vector3(165f, 0f, 132f));
            CreateWatchtower(terrain, palette, new Vector3(-165f, 0f, 120f));
            CreateCamp(terrain, palette, new Vector3(-118f, 0f, -142f));
        }

        static GameObject BuildPlayer(Terrain terrain)
        {
            var player = new GameObject("Eirik — Local Campaign Hero", typeof(CharacterController), typeof(NorthernLandsPlayerInput), typeof(NorthernLandsThirdPersonMotor));
            var spawn = new Vector3(0f, 0f, -86f);
            spawn.y = terrain.SampleHeight(spawn) + terrain.transform.position.y + 0.1f;
            player.transform.position = spawn;

            var controller = player.GetComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.42f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.42f;

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(k_CharacterPrefab);
            if (!source)
            {
                throw new InvalidOperationException($"Northern Lands character prefab was not found: {k_CharacterPrefab}");
            }

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            visual.name = "Eirik Visual — Boss Room Tank Boy";
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            foreach (var behaviour in visual.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Object.DestroyImmediate(behaviour);
            }

            foreach (var oldAnimator in visual.GetComponentsInChildren<Animator>(true))
            {
                Object.DestroyImmediate(oldAnimator);
            }

            var animator = visual.AddComponent<Animator>();
            animator.avatar = AssetDatabase.LoadAllAssetsAtPath(k_AvatarPath).OfType<Avatar>().FirstOrDefault();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(k_ControllerPath);
            animator.applyRootMotion = false;
            return player;
        }

        static void BuildCamera(Transform player)
        {
            var cameraObject = new GameObject("Northern Lands Third Person Camera", typeof(Camera), typeof(AudioListener), typeof(NorthernLandsOrbitCamera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = player.position + new Vector3(-4f, 3f, -6f);
            var camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 58f;
            camera.nearClipPlane = 0.12f;
            camera.farClipPlane = 650f;
            cameraObject.GetComponent<NorthernLandsOrbitCamera>().SetTarget(player);
        }

        static void BuildRuntimeSystems()
        {
            new GameObject("Northern Lands Runtime", typeof(NorthernLandsMobileHud));
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<EventSystem>().sendNavigationEvents = true;
        }

        static void CreateHouse(Transform parent, Terrain terrain, Palette palette, Vector3 position, float width, float yaw)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var house = new GameObject("Nordic House");
            house.transform.SetParent(parent);
            house.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            CreatePrimitive("Timber Walls", PrimitiveType.Cube, house.transform, new Vector3(0f, 2.4f, 0f), new Vector3(width, 4.8f, 6.5f), palette.Timber);
            var roofA = CreatePrimitive("Roof A", PrimitiveType.Cube, house.transform, new Vector3(0f, 5.2f, 1.65f), new Vector3(width + 1.2f, 0.55f, 4.2f), palette.Roof);
            roofA.transform.localRotation = Quaternion.Euler(27f, 0f, 0f);
            var roofB = CreatePrimitive("Roof B", PrimitiveType.Cube, house.transform, new Vector3(0f, 5.2f, -1.65f), new Vector3(width + 1.2f, 0.55f, 4.2f), palette.Roof);
            roofB.transform.localRotation = Quaternion.Euler(-27f, 0f, 0f);
            CreatePrimitive("Stone Foundation", PrimitiveType.Cube, house.transform, new Vector3(0f, 0.35f, 0f), new Vector3(width + 0.3f, 0.7f, 6.8f), palette.Stone);
        }

        static void CreateKeep(Transform parent, Terrain terrain, Palette palette, Vector3 position)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var keep = new GameObject("Riverholm Jarl Hall");
            keep.transform.SetParent(parent);
            keep.transform.position = position;
            CreatePrimitive("Great Hall", PrimitiveType.Cube, keep.transform, new Vector3(0f, 4f, 0f), new Vector3(22f, 8f, 14f), palette.DarkStone);
            CreatePrimitive("Tower Left", PrimitiveType.Cylinder, keep.transform, new Vector3(-13f, 5f, 0f), new Vector3(7f, 5f, 7f), palette.Stone);
            CreatePrimitive("Tower Right", PrimitiveType.Cylinder, keep.transform, new Vector3(13f, 5f, 0f), new Vector3(7f, 5f, 7f), palette.Stone);
        }

        static void CreateGate(Transform parent, Terrain terrain, Palette palette, Vector3 position)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var gate = new GameObject("South Gate");
            gate.transform.SetParent(parent);
            gate.transform.position = position;
            CreatePrimitive("Left Gatehouse", PrimitiveType.Cube, gate.transform, new Vector3(-9f, 4f, 0f), new Vector3(8f, 8f, 7f), palette.Stone);
            CreatePrimitive("Right Gatehouse", PrimitiveType.Cube, gate.transform, new Vector3(9f, 4f, 0f), new Vector3(8f, 8f, 7f), palette.Stone);
            CreatePrimitive("Gate Beam", PrimitiveType.Cube, gate.transform, new Vector3(0f, 8.5f, 0f), new Vector3(12f, 3f, 5f), palette.DarkStone);
        }

        static void CreateWallBlock(Transform parent, Terrain terrain, Material material, Vector3 position, Vector3 scale)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y + scale.y * 0.5f;
            var block = CreatePrimitive("Town Wall", PrimitiveType.Cube, parent, position, scale, material, true);
            GameObjectUtility.SetStaticEditorFlags(block, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
        }

        static void CreateRoad(Terrain terrain, Material material, Vector3 start, Vector3 end, float width)
        {
            var midpoint = Vector3.Lerp(start, end, 0.5f);
            midpoint.y = terrain.SampleHeight(midpoint) + terrain.transform.position.y + 0.12f;
            var length = Vector3.Distance(start, end);
            var road = CreatePrimitive("Packed Earth Road", PrimitiveType.Cube, null, midpoint, new Vector3(width, 0.18f, length), material, true);
            road.transform.rotation = Quaternion.LookRotation(end - start, Vector3.up);
            GameObjectUtility.SetStaticEditorFlags(road, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
        }

        static void CreateBridge(Vector3 position)
        {
            var palette = BuildPalette();
            var bridge = new GameObject("Riverholm Timber Bridge");
            bridge.transform.position = position;
            for (var i = -7; i <= 7; i++)
            {
                CreatePrimitive("Bridge Plank", PrimitiveType.Cube, bridge.transform, new Vector3(i * 2.2f, 0f, 0f), new Vector3(2f, 0.35f, 7f), palette.Timber);
            }
        }

        static void CreatePine(Transform parent, Terrain terrain, Palette palette, Vector3 position, float scale)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var tree = new GameObject("Black Pine");
            tree.transform.SetParent(parent);
            tree.transform.position = position;
            CreatePrimitive("Trunk", PrimitiveType.Cylinder, tree.transform, new Vector3(0f, 2.3f * scale, 0f), new Vector3(0.65f * scale, 2.3f * scale, 0.65f * scale), palette.Timber);
            CreatePrimitive("Crown Low", PrimitiveType.Sphere, tree.transform, new Vector3(0f, 5.2f * scale, 0f), new Vector3(4f, 5.8f, 4f) * scale, palette.Pine);
            CreatePrimitive("Crown High", PrimitiveType.Sphere, tree.transform, new Vector3(0f, 8.1f * scale, 0f), new Vector3(2.8f, 4.4f, 2.8f) * scale, palette.Pine);
        }

        static void CreatePortal(Terrain terrain, Palette palette, Vector3 position)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var portal = new GameObject("Dormant Dead World Portal");
            portal.transform.position = position;
            CreatePrimitive("Left Runestone", PrimitiveType.Cube, portal.transform, new Vector3(-4f, 4f, 0f), new Vector3(2.4f, 8f, 2.4f), palette.DarkStone);
            CreatePrimitive("Right Runestone", PrimitiveType.Cube, portal.transform, new Vector3(4f, 4f, 0f), new Vector3(2.4f, 8f, 2.4f), palette.DarkStone);
            CreatePrimitive("Crown Stone", PrimitiveType.Cube, portal.transform, new Vector3(0f, 8f, 0f), new Vector3(10f, 2.4f, 2.4f), palette.DarkStone);
            CreatePrimitive("Portal Veil", PrimitiveType.Cube, portal.transform, new Vector3(0f, 4.3f, 0f), new Vector3(5.7f, 6.4f, 0.35f), palette.Portal);
        }

        static void CreateWatchtower(Terrain terrain, Palette palette, Vector3 position)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var tower = new GameObject("Old Watchtower");
            tower.transform.position = position;
            CreatePrimitive("Stone Tower", PrimitiveType.Cylinder, tower.transform, new Vector3(0f, 7f, 0f), new Vector3(8f, 7f, 8f), palette.Stone);
            CreatePrimitive("Beacon", PrimitiveType.Cylinder, tower.transform, new Vector3(0f, 14.8f, 0f), new Vector3(5f, 0.6f, 5f), palette.DarkStone);
        }

        static void CreateCamp(Terrain terrain, Palette palette, Vector3 position)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var camp = new GameObject("Hunter Camp");
            camp.transform.position = position;
            CreatePrimitive("Campfire Ring", PrimitiveType.Cylinder, camp.transform, new Vector3(0f, 0.25f, 0f), new Vector3(2f, 0.25f, 2f), palette.Stone);
            CreatePrimitive("Tent", PrimitiveType.Cube, camp.transform, new Vector3(4f, 1.3f, 1f), new Vector3(4f, 2.6f, 5f), palette.Roof).transform.localRotation = Quaternion.Euler(0f, 18f, 8f);
        }

        static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material, bool worldPosition = false)
        {
            var item = GameObject.CreatePrimitive(type);
            item.name = name;
            if (parent)
            {
                item.transform.SetParent(parent, false);
            }

            if (worldPosition)
            {
                item.transform.position = position;
            }
            else
            {
                item.transform.localPosition = position;
            }

            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }

        static Material Material(string name, Color color, bool transparent = false)
        {
            var path = $"{k_GeneratedFolder}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing)
            {
                existing.color = color;
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (transparent)
            {
                material.renderQueue = 3000;
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static TerrainLayer BuildTerrainLayer()
        {
            AssetDatabase.DeleteAsset(k_TerrainLayerPath);
            AssetDatabase.DeleteAsset(k_TerrainTexturePath);

            var texture = new Texture2D(4, 4, TextureFormat.RGB24, false, true)
            {
                name = "Riverholm Moss and Earth",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color[16];
            for (var i = 0; i < pixels.Length; i++)
            {
                var variation = (i % 3) * 0.012f;
                pixels[i] = new Color(0.115f + variation, 0.18f + variation, 0.12f + variation);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, k_TerrainTexturePath);

            var layer = new TerrainLayer
            {
                name = "Riverholm Moss and Earth",
                diffuseTexture = texture,
                tileSize = new Vector2(18f, 18f),
                metallic = 0f,
                smoothness = 0.06f
            };
            AssetDatabase.CreateAsset(layer, k_TerrainLayerPath);
            return layer;
        }

        static void AddSceneToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(scene => !string.Equals(scene.path, ScenePath, StringComparison.Ordinal)))
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var separator = path.LastIndexOf('/');
            var parent = path.Substring(0, separator);
            var name = path.Substring(separator + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        sealed class Palette
        {
            public Material Stone;
            public Material DarkStone;
            public Material Timber;
            public Material Roof;
            public Material Pine;
            public Material Water;
            public Material Road;
            public Material Portal;
        }
    }
}
