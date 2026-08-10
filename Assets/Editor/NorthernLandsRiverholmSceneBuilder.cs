using System;
using System.Linq;
using Unity.BossRoom.Gameplay.NorthernLands.Combat;
using Unity.BossRoom.Gameplay.NorthernLands.Campaign;
using Unity.BossRoom.Gameplay.NorthernLands.Content;
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
        public const string DeadWorldScenePath = "Assets/Scenes/DeadWorld.unity";
        public const string TowerScenePath = "Assets/Scenes/TowerOfGods.unity";

        const string k_GeneratedFolder = "Assets/NorthernLands/Generated";
        const string k_TerrainPath = k_GeneratedFolder + "/RiverholmTerrain.asset";
        const string k_TerrainTexturePath = k_GeneratedFolder + "/RiverholmGround.asset";
        const string k_TerrainLayerPath = k_GeneratedFolder + "/RiverholmGroundLayer.terrainlayer";
        const string k_DeadTerrainPath = k_GeneratedFolder + "/DeadWorldTerrain.asset";
        const string k_DeadTerrainTexturePath = k_GeneratedFolder + "/DeadWorldGround.asset";
        const string k_DeadTerrainLayerPath = k_GeneratedFolder + "/DeadWorldGroundLayer.terrainlayer";
        const string k_TowerTerrainPath = k_GeneratedFolder + "/TowerTerrain.asset";
        const string k_TowerTerrainTexturePath = k_GeneratedFolder + "/TowerGround.asset";
        const string k_TowerTerrainLayerPath = k_GeneratedFolder + "/TowerGroundLayer.terrainlayer";
        const string k_CharacterPrefab = "Assets/Prefabs/CharGFX/CharacterGraphics/PlayerGraphics_Tank_Boy.prefab";
        const string k_JarlPrefab = "Assets/Prefabs/CharGFX/CharacterGraphics/PlayerGraphics_Tank_Girl.prefab";
        const string k_EnemyPrefab = "Assets/Prefabs/CharGFX/ImpGraphics.prefab";
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
            var jarl = BuildRiverholm(terrain, palette);
            var portal = BuildForestAndLandmarks(terrain, palette);
            var player = BuildPlayer(terrain);
            BuildEnemies(terrain);
            BuildCamera(player.transform);
            BuildRuntimeSystems(NorthernWorldId.NorthernLands, player, jarl, portal);

            EditorSceneManager.SaveScene(scene, ScenePath);
            BuildDeadWorldScene();
            BuildTowerScene();
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Northern Lands: rebuilt Riverholm and Dead World campaign scenes with Android controls.");
        }

        static void BuildDeadWorldScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureDeadWorldLighting();
            var terrain = BuildDeadWorldTerrain();
            var palette = BuildDeadWorldPalette();
            var towerPortal = BuildDeadWorldLandmarks(terrain, palette);
            var returnPortal = CreatePortal(
                terrain,
                palette,
                new Vector3(0f, 0f, -138f),
                NorthernWorldId.NorthernLands,
                true,
                "Living World Return Portal");
            var player = BuildPlayer(terrain, new Vector3(0f, 0f, -112f));
            BuildEnemies(terrain, DeadWorldEnemyPositions(), "Lost Souls — Local AI", "Lost Soul");
            BuildCamera(player.transform);
            BuildRuntimeSystems(NorthernWorldId.DeadWorld, player, null, towerPortal, returnPortal);
            EditorSceneManager.SaveScene(scene, DeadWorldScenePath);
        }

        static void BuildTowerScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureTowerLighting();
            var terrain = BuildTowerTerrain();
            var palette = BuildTowerPalette();
            BuildTowerLandmarks(terrain, palette);
            var lifeGate = CreatePortal(
                terrain,
                palette,
                new Vector3(0f, 0f, 92f),
                NorthernWorldId.NorthernLands,
                false,
                "Gate of Life");
            var divineVoice = CreateDivineVoice(terrain, new Vector3(0f, 0f, -58f));
            var player = BuildPlayer(terrain, new Vector3(0f, 0f, -82f));
            BuildEnemies(terrain, TowerEnemyPositions(), "Tower Guardians — Local AI", "Tower Guardian");
            BuildCamera(player.transform);
            BuildRuntimeSystems(NorthernWorldId.TowerOfGods, player, null, lifeGate, null, divineVoice);
            EditorSceneManager.SaveScene(scene, TowerScenePath);
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

        static void ConfigureDeadWorldLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.09f, 0.055f, 0.14f);
            RenderSettings.ambientEquatorColor = new Color(0.055f, 0.045f, 0.08f);
            RenderSettings.ambientGroundColor = new Color(0.025f, 0.02f, 0.035f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.13f, 0.09f, 0.18f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;

            var moon = new GameObject("Dead Moon", typeof(Light));
            moon.transform.rotation = Quaternion.Euler(52f, 32f, 0f);
            var light = moon.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.55f, 0.42f, 0.8f);
            light.intensity = 0.75f;
            light.shadows = LightShadows.Soft;
        }

        static void ConfigureTowerLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.18f, 0.2f, 0.28f);
            RenderSettings.ambientEquatorColor = new Color(0.11f, 0.12f, 0.17f);
            RenderSettings.ambientGroundColor = new Color(0.045f, 0.05f, 0.075f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.17f, 0.18f, 0.25f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 75f;
            RenderSettings.fogEndDistance = 210f;

            var lightObject = new GameObject("Tower Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(38f, -18f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.72f, 0.8f, 1f);
            light.intensity = 1.05f;
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

        static Terrain BuildDeadWorldTerrain()
        {
            AssetDatabase.DeleteAsset(k_DeadTerrainPath);
            var data = new TerrainData
            {
                heightmapResolution = 257,
                alphamapResolution = 256,
                baseMapResolution = 256,
                size = new Vector3(360f, 38f, 360f)
            };

            var resolution = data.heightmapResolution;
            var heights = new float[resolution, resolution];
            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var nx = x / (float)(resolution - 1);
                    var nz = z / (float)(resolution - 1);
                    var ridges = Mathf.Abs(Mathf.PerlinNoise(nx * 5.2f + 31f, nz * 5.2f + 17f) - 0.5f) * 0.24f;
                    var detail = Mathf.PerlinNoise(nx * 16f + 5f, nz * 16f + 9f) * 0.045f;
                    var pathDistance = Mathf.Abs(nx - 0.5f);
                    var pathBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.025f, 0.12f, pathDistance));
                    heights[z, x] = Mathf.Lerp(0.045f, 0.055f + ridges + detail, pathBlend);
                }
            }

            data.SetHeights(0, 0, heights);
            data.terrainLayers = new[] { BuildDeadWorldTerrainLayer() };
            AssetDatabase.CreateAsset(data, k_DeadTerrainPath);
            var terrainObject = Terrain.CreateTerrainGameObject(data);
            terrainObject.name = "Shore of the Forgotten — 360m Explorable Terrain";
            terrainObject.transform.position = new Vector3(-180f, -2f, -180f);
            var terrain = terrainObject.GetComponent<Terrain>();
            terrain.heightmapPixelError = 12f;
            terrain.basemapDistance = 240f;
            terrain.drawInstanced = true;
            GameObjectUtility.SetStaticEditorFlags(terrainObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
            return terrain;
        }

        static Terrain BuildTowerTerrain()
        {
            AssetDatabase.DeleteAsset(k_TowerTerrainPath);
            var data = new TerrainData
            {
                heightmapResolution = 129,
                alphamapResolution = 128,
                baseMapResolution = 128,
                size = new Vector3(240f, 16f, 240f)
            };

            var resolution = data.heightmapResolution;
            var heights = new float[resolution, resolution];
            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var nx = x / (float)(resolution - 1);
                    var nz = z / (float)(resolution - 1);
                    var edge = Mathf.Max(Mathf.Abs(nx - 0.5f), Mathf.Abs(nz - 0.5f));
                    var rim = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.34f, 0.5f, edge));
                    heights[z, x] = 0.055f + rim * 0.19f;
                }
            }

            data.SetHeights(0, 0, heights);
            data.terrainLayers = new[] { BuildTowerTerrainLayer() };
            AssetDatabase.CreateAsset(data, k_TowerTerrainPath);
            var terrainObject = Terrain.CreateTerrainGameObject(data);
            terrainObject.name = "Tower of Gods — Trial Hall Terrain";
            terrainObject.transform.position = new Vector3(-120f, -2f, -120f);
            var terrain = terrainObject.GetComponent<Terrain>();
            terrain.heightmapPixelError = 8f;
            terrain.basemapDistance = 190f;
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

        static Palette BuildDeadWorldPalette()
        {
            return new Palette
            {
                Stone = Material("NL_DeadStone", new Color(0.16f, 0.13f, 0.2f)),
                DarkStone = Material("NL_DeadObsidian", new Color(0.045f, 0.035f, 0.065f)),
                Timber = Material("NL_DeadBone", new Color(0.5f, 0.46f, 0.52f)),
                Roof = Material("NL_DeadRuin", new Color(0.11f, 0.075f, 0.14f)),
                Pine = Material("NL_DeadGrowth", new Color(0.13f, 0.055f, 0.18f)),
                Water = Material("NL_SoulRiver", new Color(0.34f, 0.12f, 0.52f, 0.8f), true),
                Road = Material("NL_DeadPath", new Color(0.25f, 0.2f, 0.29f)),
                Portal = Material("NL_DeadPortal", new Color(0.48f, 0.24f, 0.9f), true)
            };
        }

        static Palette BuildTowerPalette()
        {
            return new Palette
            {
                Stone = Material("NL_TowerMarble", new Color(0.33f, 0.36f, 0.44f)),
                DarkStone = Material("NL_TowerDark", new Color(0.075f, 0.085f, 0.12f)),
                Timber = Material("NL_TowerGold", new Color(0.57f, 0.42f, 0.13f)),
                Roof = Material("NL_TowerBlue", new Color(0.11f, 0.19f, 0.35f)),
                Pine = Material("NL_TowerCrystal", new Color(0.23f, 0.55f, 0.92f)),
                Water = Material("NL_TowerLight", new Color(0.32f, 0.63f, 1f, 0.72f), true),
                Road = Material("NL_TowerFloor", new Color(0.2f, 0.22f, 0.28f)),
                Portal = Material("NL_LifeGate", new Color(0.35f, 0.78f, 1f), true)
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

        static NorthernLandsJarlNpc BuildRiverholm(Terrain terrain, Palette palette)
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
            return CreateJarl(terrain, new Vector3(0f, 0f, 7f));
        }

        static NorthernLandsWorldPortal BuildForestAndLandmarks(Terrain terrain, Palette palette)
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

            var portal = CreatePortal(
                terrain,
                palette,
                new Vector3(165f, 0f, 132f),
                NorthernWorldId.DeadWorld,
                false,
                "Dormant Dead World Portal");
            CreateWatchtower(terrain, palette, new Vector3(-165f, 0f, 120f));
            CreateCamp(terrain, palette, new Vector3(-118f, 0f, -142f));
            return portal;
        }

        static GameObject BuildPlayer(Terrain terrain, Vector3? requestedSpawn = null)
        {
            var player = new GameObject(
                "Eirik — Local Campaign Hero",
                typeof(CharacterController),
                typeof(NorthernLandsPlayerInput),
                typeof(NorthernLandsCombatant),
                typeof(NorthernLandsPlayerCombat),
                typeof(NorthernLandsThirdPersonMotor));
            var spawn = requestedSpawn ?? new Vector3(0f, 0f, -86f);
            spawn.y = terrain.SampleHeight(spawn) + terrain.transform.position.y + 0.1f;
            player.transform.position = spawn;

            var controller = player.GetComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.42f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.42f;
            player.GetComponent<NorthernLandsCombatant>().Configure(120f, true);

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

        static void BuildEnemies(Terrain terrain)
        {
            var positions = new[]
            {
                new Vector3(-20f, 0f, -112f), new Vector3(28f, 0f, -125f),
                new Vector3(-86f, 0f, -98f), new Vector3(102f, 0f, -88f),
                new Vector3(-124f, 0f, -148f), new Vector3(-108f, 0f, -132f),
                new Vector3(142f, 0f, 102f), new Vector3(173f, 0f, 126f),
                new Vector3(-174f, 0f, 105f), new Vector3(-145f, 0f, 143f),
                new Vector3(118f, 0f, 170f), new Vector3(185f, 0f, -35f)
            };
            BuildEnemies(terrain, positions, "Riverholm Hostiles — Local AI", "Frost Imp");
        }

        static Vector3[] DeadWorldEnemyPositions()
        {
            return new[]
            {
                new Vector3(-24f, 0f, -62f), new Vector3(28f, 0f, -44f),
                new Vector3(-55f, 0f, -8f), new Vector3(61f, 0f, 3f),
                new Vector3(-18f, 0f, 38f), new Vector3(20f, 0f, 55f),
                new Vector3(-82f, 0f, 78f), new Vector3(88f, 0f, 91f),
                new Vector3(-36f, 0f, 126f), new Vector3(43f, 0f, 137f)
            };
        }

        static Vector3[] TowerEnemyPositions()
        {
            return new[]
            {
                new Vector3(-26f, 0f, -38f), new Vector3(26f, 0f, -38f),
                new Vector3(-42f, 0f, 0f), new Vector3(42f, 0f, 0f),
                new Vector3(-28f, 0f, 38f), new Vector3(28f, 0f, 38f),
                new Vector3(-12f, 0f, 67f), new Vector3(12f, 0f, 67f)
            };
        }

        static void BuildEnemies(Terrain terrain, Vector3[] positions, string groupName, string enemyName)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(k_EnemyPrefab);
            if (!source)
            {
                throw new InvalidOperationException($"Northern Lands enemy graphics prefab was not found: {k_EnemyPrefab}");
            }

            var group = new GameObject(groupName);
            for (var i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                position.y = terrain.SampleHeight(position) + terrain.transform.position.y + 0.05f;
                var enemy = new GameObject(
                    $"{enemyName} {i + 1:00}",
                    typeof(CharacterController),
                    typeof(NorthernLandsCombatant),
                    typeof(NorthernLandsEnemyAI));
                enemy.transform.SetParent(group.transform);
                enemy.transform.position = position;
                var controller = enemy.GetComponent<CharacterController>();
                controller.height = 1.75f;
                controller.radius = 0.42f;
                controller.center = new Vector3(0f, 0.88f, 0f);
                controller.stepOffset = 0.35f;
                enemy.GetComponent<NorthernLandsCombatant>().Configure(70f, false);

                var visual = (GameObject)PrefabUtility.InstantiatePrefab(source);
                PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                visual.name = $"{enemyName} Visual";
                visual.transform.SetParent(enemy.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                foreach (var behaviour in visual.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    Object.DestroyImmediate(behaviour);
                }
            }
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

        static void BuildRuntimeSystems(
            NorthernWorldId world,
            GameObject player,
            NorthernLandsJarlNpc jarl,
            NorthernLandsWorldPortal portal,
            NorthernLandsWorldPortal returnPortal = null,
            NorthernLandsDivineVoiceNpc divineVoice = null)
        {
            var runtime = new GameObject("Northern Lands Runtime", typeof(NorthernLandsCampaignDirector), typeof(NorthernLandsMobileHud));
            runtime.GetComponent<NorthernLandsCampaignDirector>().Configure(
                world,
                player.transform,
                player.GetComponent<NorthernLandsCombatant>(),
                jarl,
                portal,
                returnPortal,
                divineVoice);
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<EventSystem>().sendNavigationEvents = true;
        }

        static NorthernLandsJarlNpc CreateJarl(Terrain terrain, Vector3 position)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var jarl = new GameObject("Jarl Ingvar — Riverholm Quest Giver", typeof(NorthernLandsJarlNpc));
            jarl.transform.position = position;
            jarl.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(k_JarlPrefab);
            if (!source)
            {
                throw new InvalidOperationException($"Northern Lands jarl prefab was not found: {k_JarlPrefab}");
            }

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            visual.name = "Jarl Ingvar Visual";
            visual.transform.SetParent(jarl.transform, false);
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
            return jarl.GetComponent<NorthernLandsJarlNpc>();
        }

        static NorthernLandsDivineVoiceNpc CreateDivineVoice(Terrain terrain, Vector3 position)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var voice = new GameObject("Voice of the Gods — Trial Guide", typeof(NorthernLandsDivineVoiceNpc));
            voice.transform.position = position;
            voice.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(k_JarlPrefab);
            if (!source)
            {
                throw new InvalidOperationException($"Northern Lands divine voice prefab was not found: {k_JarlPrefab}");
            }

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            visual.name = "Divine Voice Visual";
            visual.transform.SetParent(voice.transform, false);
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
            return voice.GetComponent<NorthernLandsDivineVoiceNpc>();
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

        static NorthernLandsWorldPortal BuildDeadWorldLandmarks(Terrain terrain, Palette palette)
        {
            CreateRoad(terrain, palette.Road, new Vector3(0f, 0f, -145f), new Vector3(0f, 0f, 145f), 7f);

            var river = GameObject.CreatePrimitive(PrimitiveType.Plane);
            river.name = "River of Lost Souls";
            river.transform.SetPositionAndRotation(new Vector3(-102f, 0.75f, 5f), Quaternion.identity);
            river.transform.localScale = new Vector3(2.2f, 1f, 35f);
            river.GetComponent<Renderer>().sharedMaterial = palette.Water;
            Object.DestroyImmediate(river.GetComponent<Collider>());

            var ruins = new GameObject("Ruins of the Forgotten Court");
            var ruinPositions = new[]
            {
                new Vector3(-55f, 0f, -18f), new Vector3(52f, 0f, -12f),
                new Vector3(-70f, 0f, 52f), new Vector3(68f, 0f, 61f),
                new Vector3(-42f, 0f, 118f), new Vector3(46f, 0f, 121f)
            };
            for (var i = 0; i < ruinPositions.Length; i++)
            {
                var position = ruinPositions[i];
                position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
                var ruin = new GameObject($"Broken Shrine {i + 1:00}");
                ruin.transform.SetParent(ruins.transform);
                ruin.transform.position = position;
                ruin.transform.rotation = Quaternion.Euler(0f, i * 37f, 0f);
                CreatePrimitive("Broken Pillar", PrimitiveType.Cylinder, ruin.transform, new Vector3(-3f, 3f, 0f), new Vector3(1.25f, 3f, 1.25f), palette.Stone);
                CreatePrimitive("Leaning Pillar", PrimitiveType.Cylinder, ruin.transform, new Vector3(3f, 2.2f, 0f), new Vector3(1.1f, 2.2f, 1.1f), palette.Stone).transform.localRotation = Quaternion.Euler(0f, 0f, 17f);
                CreatePrimitive("Altar", PrimitiveType.Cube, ruin.transform, new Vector3(0f, 0.7f, 2.8f), new Vector3(5f, 1.4f, 2.2f), palette.DarkStone);
            }

            var gate = new GameObject("Sealed Road to the Tower of Gods");
            gate.transform.position = new Vector3(0f, terrain.SampleHeight(new Vector3(0f, 0f, 154f)) + terrain.transform.position.y, 154f);
            CreatePrimitive("Left Obelisk", PrimitiveType.Cube, gate.transform, new Vector3(-8f, 7f, 0f), new Vector3(4f, 14f, 4f), palette.DarkStone);
            CreatePrimitive("Right Obelisk", PrimitiveType.Cube, gate.transform, new Vector3(8f, 7f, 0f), new Vector3(4f, 14f, 4f), palette.DarkStone);
            CreatePrimitive("Soul Seal", PrimitiveType.Sphere, gate.transform, new Vector3(0f, 8f, 0f), new Vector3(5f, 7f, 1.2f), palette.Portal);

            var towerPortal = CreatePortal(
                terrain,
                palette,
                new Vector3(0f, 0f, 148f),
                NorthernWorldId.TowerOfGods,
                false,
                "Road to the Tower of Gods");

            var bones = new GameObject("Bone Fields");
            var random = new System.Random(14015);
            for (var i = 0; i < 44; i++)
            {
                var position = new Vector3((float)(random.NextDouble() * 300d - 150d), 0f, (float)(random.NextDouble() * 300d - 150d));
                if (Mathf.Abs(position.x) < 16f)
                {
                    continue;
                }
                position.y = terrain.SampleHeight(position) + terrain.transform.position.y + 0.35f;
                var bone = CreatePrimitive("Ancient Bone", PrimitiveType.Cylinder, bones.transform, position, new Vector3(0.24f, 1.2f, 0.24f), palette.Timber, true);
                bone.transform.rotation = Quaternion.Euler(70f, i * 29f, 12f);
                Object.DestroyImmediate(bone.GetComponent<Collider>());
            }

            return towerPortal;
        }

        static void BuildTowerLandmarks(Terrain terrain, Palette palette)
        {
            var hall = new GameObject("Hall of the Divine Trial");
            var floorPosition = new Vector3(0f, terrain.SampleHeight(Vector3.zero) + terrain.transform.position.y + 0.2f, 0f);
            CreatePrimitive("Trial Floor", PrimitiveType.Cylinder, hall.transform, floorPosition, new Vector3(43f, 0.45f, 43f), palette.Road, true);

            for (var i = 0; i < 12; i++)
            {
                var angle = i * Mathf.PI * 2f / 12f;
                var position = new Vector3(Mathf.Cos(angle) * 66f, 0f, Mathf.Sin(angle) * 66f);
                position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
                var pillar = new GameObject($"Divine Pillar {i + 1:00}");
                pillar.transform.SetParent(hall.transform);
                pillar.transform.position = position;
                CreatePrimitive("Marble Shaft", PrimitiveType.Cylinder, pillar.transform, new Vector3(0f, 9f, 0f), new Vector3(2.5f, 9f, 2.5f), palette.Stone);
                CreatePrimitive("Golden Crown", PrimitiveType.Cylinder, pillar.transform, new Vector3(0f, 18.3f, 0f), new Vector3(3.3f, 0.45f, 3.3f), palette.Timber);
                CreatePrimitive("Blue Flame", PrimitiveType.Sphere, pillar.transform, new Vector3(0f, 20f, 0f), new Vector3(1.4f, 2.6f, 1.4f), palette.Water);
            }

            for (var i = -4; i <= 4; i++)
            {
                var z = i * 17f;
                CreatePrimitive("Left Trial Wall", PrimitiveType.Cube, hall.transform, new Vector3(-88f, 5f, z), new Vector3(5f, 10f, 14f), palette.DarkStone, true);
                CreatePrimitive("Right Trial Wall", PrimitiveType.Cube, hall.transform, new Vector3(88f, 5f, z), new Vector3(5f, 10f, 14f), palette.DarkStone, true);
            }

            var daisPosition = new Vector3(0f, terrain.SampleHeight(new Vector3(0f, 0f, 78f)) + terrain.transform.position.y, 78f);
            var dais = new GameObject("Gate of Life Dais");
            dais.transform.position = daisPosition;
            for (var step = 0; step < 4; step++)
            {
                CreatePrimitive("Dais Step", PrimitiveType.Cube, dais.transform, new Vector3(0f, step * 0.55f, step * 1.2f), new Vector3(20f - step * 2f, 0.55f, 9f - step), palette.Stone);
            }

            var crystals = new GameObject("Aether Crystals");
            var points = new[]
            {
                new Vector3(-72f, 0f, -72f), new Vector3(72f, 0f, -72f),
                new Vector3(-74f, 0f, 70f), new Vector3(74f, 0f, 70f)
            };
            for (var i = 0; i < points.Length; i++)
            {
                var position = points[i];
                position.y = terrain.SampleHeight(position) + terrain.transform.position.y + 4f;
                var crystal = CreatePrimitive("Aether Crystal", PrimitiveType.Cube, crystals.transform, position, new Vector3(3f, 9f, 3f), palette.Pine, true);
                crystal.transform.rotation = Quaternion.Euler(0f, i * 41f, 45f);
            }
        }

        static NorthernLandsWorldPortal CreatePortal(
            Terrain terrain,
            Palette palette,
            Vector3 position,
            NorthernWorldId destination,
            bool unlocked,
            string portalName)
        {
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            var portal = new GameObject(portalName, typeof(NorthernLandsWorldPortal));
            portal.transform.position = position;
            CreatePrimitive("Left Runestone", PrimitiveType.Cube, portal.transform, new Vector3(-4f, 4f, 0f), new Vector3(2.4f, 8f, 2.4f), palette.DarkStone);
            CreatePrimitive("Right Runestone", PrimitiveType.Cube, portal.transform, new Vector3(4f, 4f, 0f), new Vector3(2.4f, 8f, 2.4f), palette.DarkStone);
            CreatePrimitive("Crown Stone", PrimitiveType.Cube, portal.transform, new Vector3(0f, 8f, 0f), new Vector3(10f, 2.4f, 2.4f), palette.DarkStone);
            CreatePrimitive("Portal Veil", PrimitiveType.Cube, portal.transform, new Vector3(0f, 4.3f, 0f), new Vector3(5.7f, 6.4f, 0.35f), palette.Portal);
            var endpoint = portal.GetComponent<NorthernLandsWorldPortal>();
            endpoint.Configure(destination, unlocked);
            return endpoint;
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

        static TerrainLayer BuildDeadWorldTerrainLayer()
        {
            AssetDatabase.DeleteAsset(k_DeadTerrainLayerPath);
            AssetDatabase.DeleteAsset(k_DeadTerrainTexturePath);

            var texture = new Texture2D(4, 4, TextureFormat.RGB24, false, true)
            {
                name = "Dead World Ash and Stone",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color[16];
            for (var i = 0; i < pixels.Length; i++)
            {
                var variation = (i % 4) * 0.009f;
                pixels[i] = new Color(0.085f + variation, 0.065f + variation, 0.105f + variation);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, k_DeadTerrainTexturePath);

            var layer = new TerrainLayer
            {
                name = "Dead World Ash and Stone",
                diffuseTexture = texture,
                tileSize = new Vector2(15f, 15f),
                metallic = 0f,
                smoothness = 0.02f
            };
            AssetDatabase.CreateAsset(layer, k_DeadTerrainLayerPath);
            return layer;
        }

        static TerrainLayer BuildTowerTerrainLayer()
        {
            AssetDatabase.DeleteAsset(k_TowerTerrainLayerPath);
            AssetDatabase.DeleteAsset(k_TowerTerrainTexturePath);

            var texture = new Texture2D(4, 4, TextureFormat.RGB24, false, true)
            {
                name = "Tower Marble Floor",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color[16];
            for (var i = 0; i < pixels.Length; i++)
            {
                var line = i % 4 == 0 ? 0.035f : 0f;
                pixels[i] = new Color(0.18f + line, 0.19f + line, 0.23f + line);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            AssetDatabase.CreateAsset(texture, k_TowerTerrainTexturePath);

            var layer = new TerrainLayer
            {
                name = "Tower Marble Floor",
                diffuseTexture = texture,
                tileSize = new Vector2(12f, 12f),
                metallic = 0.05f,
                smoothness = 0.22f
            };
            AssetDatabase.CreateAsset(layer, k_TowerTerrainLayerPath);
            return layer;
        }

        static void AddSceneToBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            var changed = false;
            foreach (var path in new[] { ScenePath, DeadWorldScenePath, TowerScenePath })
            {
                if (scenes.All(scene => !string.Equals(scene.path, path, StringComparison.Ordinal)))
                {
                    scenes.Add(new EditorBuildSettingsScene(path, true));
                    changed = true;
                }
            }

            if (changed)
            {
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
