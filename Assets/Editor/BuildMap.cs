using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public static class BuildMap
{
    const float T = 0.64f;        // tile size (64px @ 100 PPU)
    const float H = 0.32f;        // half tile
    const int TOTAL = 157;        // tiles for ~100 units

    static string TileDir => "Assets/kenney_new-platformer-pack-1.1/Sprites/Tiles/Default";
    static string BgDir => "Assets/kenney_new-platformer-pack-1.1/Sprites/Backgrounds/Default";

    static Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

    // Tiles that should have trigger colliders (no physical blockage)
    static readonly HashSet<string> TriggerTiles = new HashSet<string>
    {
        "spikes", "saw",
        "coin_gold", "gem_blue", "gem_red", "gem_yellow", "gem_green", "heart",
        "grass", "bush", "hill", "hill_top",
        "torch_on_a", "torch_on_b",
        "flag_red_a", "flag_red_b", "flag_green_a", "flag_green_b",
    };

    static Sprite LoadSprite(string dir, string name)
    {
        string path = $"{dir}/{name}.png";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) Debug.LogWarning($"Sprite not found: {path}");
        return sprite;
    }

    // ── Entry Point ──────────────────────────────────────────────────

    [MenuItem("Tools/Build 100m Parkour Map")]
    static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "gamesense1";

        // Camera
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6;
        camGO.transform.position = new Vector3(50, 5, -10);
        camGO.transform.localScale = Vector3.one;

        // Create folder & prefabs
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        CreatePrefabs();

        GameObject root = new GameObject("Level_Root");

        // Backgrounds
        BuildBackground(root.transform);

        // Ground
        BuildGround(root.transform);

        // Platforms
        BuildPlatforms(root.transform);

        // Spikes & hazards
        BuildHazards(root.transform);

        // Decorations
        BuildDecorations(root.transform);

        // Pickups
        BuildPickups(root.transform);

        // Start / End markers
        BuildStartEnd(root.transform);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/gamesense1.scene");
        Debug.Log($"<color=#2EA3FF>Map built: ~100 units, {CountChildren(root)} GameObjects</color>");
    }

    static int CountChildren(GameObject parent)
    {
        int c = 1;
        foreach (Transform t in parent.transform) c += CountChildren(t.gameObject);
        return c;
    }

    // ── Prefab Creation ──────────────────────────────────────────────

    static void CreatePrefabs()
    {
        string[] tiles = {
            "terrain_grass_block_top", "terrain_grass_block",
            "terrain_grass_horizontal_left", "terrain_grass_horizontal_middle", "terrain_grass_horizontal_right",
            "brick_brown", "brick_grey",
            "spikes", "saw",
            "bush", "grass", "hill", "hill_top",
            "sign", "sign_exit",
            "flag_red_a", "flag_red_b", "flag_green_a", "flag_green_b",
            "coin_gold", "gem_blue", "gem_red", "gem_yellow", "gem_green", "heart",
            "door_closed", "door_closed_top",
            "block_coin", "block_exclamation",
            "fence", "fence_broken",
            "torch_on_a", "torch_on_b",
        };
        foreach (var t in tiles)
            GetPrefab(t);
    }

    static GameObject GetPrefab(string name)
    {
        if (_prefabs.ContainsKey(name)) return _prefabs[name];

        var sprite = LoadSprite(TileDir, name);
        if (sprite == null) return null;

        var go = new GameObject(name, typeof(SpriteRenderer));
        go.GetComponent<SpriteRenderer>().sprite = sprite;

        // Add collider — make every prefab a real physics object
        var collider = go.AddComponent<BoxCollider2D>();
        if (TriggerTiles.Contains(name))
            collider.isTrigger = true;

        string prefabPath = $"Assets/Prefabs/{name}.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        _prefabs[name] = prefab;
        return prefab;
    }

    static GameObject Place(Transform parent, string prefabName, float x, float y, float scale = 1)
    {
        var prefab = GetPrefab(prefabName);
        if (prefab == null) return null;

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        go.transform.position = new Vector3(x, y, 0);
        if (scale != 1) go.transform.localScale = Vector3.one * scale;
        return go;
    }

    static GameObject Place(Transform parent, string prefabName, float x, float y, float sx, float sy)
    {
        var go = Place(parent, prefabName, x, y);
        if (go != null) go.transform.localScale = new Vector3(sx, sy, 1);
        return go;
    }

    static GameObject PlaceTile(Transform parent, string prefabName, int gx, int gy)
    {
        return Place(parent, prefabName, gx * T + H, gy * T + H);
    }

    // ── Map Building ─────────────────────────────────────────────────

    static void BuildBackground(Transform parent)
    {
        var bg = new GameObject("Background").transform;
        bg.SetParent(parent);

        Camera.main.backgroundColor = new Color(0.48f, 0.71f, 0.94f);

        // Layered backgrounds – load sprites from the Backgrounds folder directly
        var cloudSpr = LoadSprite(BgDir, "background_clouds");
        var colorSpr = LoadSprite(BgDir, "background_color_hills");
        var fadeSpr = LoadSprite(BgDir, "background_fade_hills");

        if (cloudSpr) MakeBgSprite(bg, cloudSpr, 50, 8, 20, 10);
        if (colorSpr) MakeBgSprite(bg, colorSpr, 50, 4, 15, 8);
        if (fadeSpr) MakeBgSprite(bg, fadeSpr, 50, 3, 12, 6);
    }

    static void MakeBgSprite(Transform parent, Sprite sprite, float x, float y, int repeatX, int repeatY)
    {
        for (int ix = 0; ix < repeatX; ix++)
        {
            for (int iy = 0; iy < repeatY; iy++)
            {
                var go = new GameObject($"bg_{sprite.name}_{ix}_{iy}", typeof(SpriteRenderer));
                go.GetComponent<SpriteRenderer>().sprite = sprite;
                go.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
                float sx = sprite.rect.width / sprite.pixelsPerUnit;
                float sy = sprite.rect.height / sprite.pixelsPerUnit;
                go.transform.SetParent(parent);
                go.transform.position = new Vector3(x + ix * sx, y + iy * sy, 0);
            }
        }
    }

    static void BuildGround(Transform parent)
    {
        var ground = new GameObject("Ground").transform;
        ground.SetParent(parent);

        // Gaps: [startTile, endTile] (inclusive range to remove)
        var gaps = new HashSet<int> { 12, 13, 14, 31, 32, 48, 49, 50,
                                      68, 69, 85, 86, 87, 103, 104, 120, 121, 122, 140, 141 };

        for (int gx = 0; gx < TOTAL; gx++)
        {
            if (gaps.Contains(gx)) continue;

            // Surface
            PlaceTile(ground, "terrain_grass_block_top", gx, 0);
            // Below surface
            PlaceTile(ground, "terrain_grass_block", gx, -1);
            PlaceTile(ground, "terrain_grass_block", gx, -2);
            PlaceTile(ground, "terrain_grass_block", gx, -3);
        }

        // Ground edge decorations
        for (int gx = 0; gx < TOTAL; gx++)
        {
            if (gaps.Contains(gx) || Random.value > 0.15f) continue;
            PlaceTile(ground, "grass", gx, 1);
        }
    }

    static void BuildPlatforms(Transform parent)
    {
        var plat = new GameObject("Platforms").transform;
        plat.SetParent(parent);

        // Section 1 (0-20): low intro platforms
        MakePlatform(plat, 5, 2, 3, "brick_brown");     // low platform
        MakePlatform(plat, 10, 3, 2, "brick_brown");    // mid

        // Section 2 (20-40): medium height
        MakePlatform(plat, 22, 3, 4, "brick_brown");
        MakePlatform(plat, 28, 5, 2, "brick_grey");
        MakePlatform(plat, 35, 2, 3, "brick_brown");

        // Section 3 (40-60): multi-layer
        MakePlatform(plat, 43, 3, 3, "brick_brown");    // low
        MakePlatform(plat, 45, 6, 3, "brick_grey");     // mid
        MakePlatform(plat, 52, 2, 2, "brick_brown");    // low left
        MakePlatform(plat, 55, 5, 2, "brick_brown");    // mid
        MakePlatform(plat, 57, 8, 3, "brick_grey");     // high
        MakePlatform(plat, 60, 4, 3, "brick_brown");

        // Section 4 (60-80): dense challenge
        MakePlatform(plat, 63, 2, 2, "brick_brown");
        MakePlatform(plat, 66, 2, 6, "brick_brown", true);  // pillar
        MakePlatform(plat, 72, 3, 4, "brick_grey");
        MakePlatform(plat, 75, 5, 2, "brick_brown");
        MakePlatform(plat, 78, 1, 2, "brick_brown");    // single stepping stone
        MakePlatform(plat, 80, 1, 2, "brick_brown");
        MakePlatform(plat, 82, 1, 2, "brick_brown");

        // Section 5 (80-100): final stretch
        MakePlatform(plat, 88, 3, 3, "brick_grey");
        MakePlatform(plat, 92, 2, 5, "brick_brown", true); // pillar
        MakePlatform(plat, 97, 4, 3, "brick_brown");
        MakePlatform(plat, 105, 3, 4, "brick_grey");
        MakePlatform(plat, 110, 2, 3, "brick_brown");
        MakePlatform(plat, 115, 5, 2, "brick_brown");
        MakePlatform(plat, 128, 2, 2, "brick_brown");
        MakePlatform(plat, 132, 3, 3, "brick_grey");

        // Stepping stone path near end
        MakePlatform(plat, 143, 1, 2, "brick_brown");
        MakePlatform(plat, 145, 1, 2, "brick_brown");
        MakePlatform(plat, 147, 1, 2, "brick_brown");
        MakePlatform(plat, 149, 1, 2, "brick_brown");
        MakePlatform(plat, 151, 2, 2, "brick_brown");
    }

    static void MakePlatform(Transform parent, int gx, int gy, int width, string tile, bool vertical = false)
    {
        if (vertical)
        {
            for (int j = 0; j < width; j++)
                PlaceTile(parent, tile, gx, gy + j);
            return;
        }
        for (int i = 0; i < width; i++)
            PlaceTile(parent, tile, gx + i, gy);
    }

    static void BuildHazards(Transform parent)
    {
        var haz = new GameObject("Hazards").transform;
        haz.SetParent(parent);

        // Spikes: place near gaps and along platforms
        var spikePositions = new (int, int)[] {
            // Near gaps in sections 2-4
            (10, 1), (15, 1), (29, 1), (33, 1), (46, 1), (51, 1),
            (67, 1), (70, 1), (83, 1), (88, 1), (106, 1), (119, 1), (123, 1),
            // Platform spikes
            (22, 4), (35, 3), (55, 6), (72, 4), (97, 4),
        };
        foreach (var (x, y) in spikePositions)
            PlaceTile(haz, "spikes", x, y);

        // Saws (mid-air hazards)
        Place(haz, "saw", 25 * T, 5 * T);
        Place(haz, "saw", 44 * T, 7 * T);
        Place(haz, "saw", 55 * T, 9 * T);
        Place(haz, "saw", 73 * T, 5 * T);
        Place(haz, "saw", 90 * T, 6 * T);
        Place(haz, "saw", 108 * T, 4 * T);
        Place(haz, "saw", 130 * T, 7 * T);
        Place(haz, "saw", 145 * T, 5 * T);
    }

    static void BuildDecorations(Transform parent)
    {
        var deco = new GameObject("Decorations").transform;
        deco.SetParent(parent);

        // Hill formations (background-ish)
        Place(deco, "hill", 8 * T + H, 1 * T + H, 2);
        Place(deco, "hill", 18 * T + H, 0 * T + H, 1.5f);
        Place(deco, "hill", 40 * T + H, 0 * T + H, 2.5f);
        Place(deco, "hill", 62 * T + H, 0 * T + H, 2);
        Place(deco, "hill", 95 * T + H, 0 * T + H, 1.8f);
        Place(deco, "hill", 135 * T + H, 0 * T + H, 2.2f);

        // Bushes on ground
        var bushPos = new int[] { 3, 7, 16, 24, 30, 38, 42, 53, 57, 65, 74, 81, 91, 100, 112, 118, 127, 138, 148 };
        foreach (var bx in bushPos)
            PlaceTile(deco, "bush", bx, 1);

        // Fences
        PlaceTile(deco, "fence", 6, 1);
        PlaceTile(deco, "fence", 26, 1);
        PlaceTile(deco, "fence", 58, 1);
        PlaceTile(deco, "fence", 76, 1);
        PlaceTile(deco, "fence", 98, 1);
        PlaceTile(deco, "fence", 114, 1);
        PlaceTile(deco, "fence", 136, 1);

        // Flags along the route
        PlaceTile(deco, "flag_red_a", 5, 2);
        PlaceTile(deco, "flag_green_a", 25, 3);
        PlaceTile(deco, "flag_red_b", 45, 7);
        PlaceTile(deco, "flag_green_b", 65, 3);
        PlaceTile(deco, "flag_red_a", 85, 2);
        PlaceTile(deco, "flag_green_a", 105, 3);
        PlaceTile(deco, "flag_red_b", 125, 2);

        // Torches on some platforms
        PlaceTile(deco, "torch_on_a", 10, 4);
        PlaceTile(deco, "torch_on_b", 35, 4);
        PlaceTile(deco, "torch_on_a", 55, 7);
        PlaceTile(deco, "torch_on_b", 72, 5);
        PlaceTile(deco, "torch_on_a", 97, 5);
        PlaceTile(deco, "torch_on_b", 128, 4);

        // Signs
        PlaceTile(deco, "sign", 2, 1);
        PlaceTile(deco, "sign_exit", 152, 1);
    }

    static void BuildPickups(Transform parent)
    {
        var pickups = new GameObject("Pickups").transform;
        pickups.SetParent(parent);

        // Coins
        var coinPos = new (int, int)[] {
            (5, 4), (6, 4), (7, 4),
            (22, 5), (23, 5), (24, 5), (25, 5),
            (43, 5), (44, 5), (45, 5),
            (55, 7), (56, 7),
            (72, 5), (73, 5), (74, 5),
            (88, 5), (89, 5),
            (97, 5), (98, 5), (99, 5),
            (115, 4), (116, 4),
            (132, 5), (133, 5), (134, 5),
        };
        foreach (var (x, y) in coinPos)
            PlaceTile(pickups, "coin_gold", x, y);

        // Gems
        PlaceTile(pickups, "gem_blue", 28, 7);
        PlaceTile(pickups, "gem_red", 45, 10);
        PlaceTile(pickups, "gem_green", 66, 8);
        PlaceTile(pickups, "gem_yellow", 92, 7);
        PlaceTile(pickups, "gem_blue", 128, 6);

        // Hearts
        PlaceTile(pickups, "heart", 45, 8);
        PlaceTile(pickups, "heart", 97, 7);
        PlaceTile(pickups, "heart", 143, 4);
    }

    static void BuildStartEnd(Transform parent)
    {
        // Start platform
        Place(parent, "sign", 0, 0.5f);

        // Exit door at end
        PlaceTile(parent, "door_closed_top", 153, 1);
        PlaceTile(parent, "door_closed", 153, 0);

        // Flags around door
        PlaceTile(parent, "flag_green_b", 151, 2);
        PlaceTile(parent, "flag_green_b", 155, 2);

        // Key near door (final pickup)
        PlaceTile(parent, "gem_yellow", 153, 3);
    }
}
