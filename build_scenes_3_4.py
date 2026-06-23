"""
Build gamesense3.scene and gamesense4.scene with different level layouts.
Communicates with Unity via MCP (Model Context Protocol) HTTP transport.
"""
import urllib.request
import json
import sys
import time


def mcp_call_raw(method, params=None):
    """Call an MCP tool and return the parsed JSON response."""
    init_data = json.dumps({
        "jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {
            "protocolVersion": "2025-03-26", "capabilities": {},
            "clientInfo": {"name": "claude-code", "version": "1.0.0"}
        }
    }).encode()
    req = urllib.request.Request(
        "http://localhost:8081/mcp",
        data=init_data,
        headers={"Content-Type": "application/json",
                 "Accept": "application/json, text/event-stream"},
        method="POST"
    )
    resp = urllib.request.urlopen(req, timeout=15)
    session_id = resp.headers.get("mcp-session-id")
    resp.read()

    call_data = json.dumps({
        "jsonrpc": "2.0", "id": 2, "method": "tools/call",
        "params": {"name": method, "arguments": params}
    }).encode()
    req2 = urllib.request.Request(
        "http://localhost:8081/mcp",
        data=call_data,
        headers={
            "Content-Type": "application/json",
            "Accept": "application/json, text/event-stream",
            "mcp-session-id": session_id
        },
        method="POST"
    )
    resp2 = urllib.request.urlopen(req2, timeout=120)
    raw = resp2.read().decode()
    for line in raw.split("\n"):
        if line.startswith("data: "):
            return json.loads(line[6:])
    return raw


def exec_csharp(code, label=""):
    """Execute C# code in Unity editor via execute_code tool."""
    r = mcp_call_raw("execute_code", {"action": "execute", "code": code})
    txt = r.get("result", {}).get("content", [{}])[0].get("text", "")
    data = json.loads(txt) if isinstance(txt, str) else txt
    success = data.get("success", False)
    msg = data.get("message", data.get("data", {}).get("result", ""))
    if not success:
        errs = data.get("data", {}).get("errors", [])
        print(f"  FAIL [{label}]: {msg} {'; '.join(errs)}")
    else:
        print(f"  OK   [{label}]: {msg[:80]}")
    return success


def create_scene(name):
    """Create a new empty scene in editor."""
    code = '''
    UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
    Debug.Log("Scene created");
    return "created";
    '''
    exec_csharp(code, "new scene")
    time.sleep(1)


def save_scene(path):
    """Save the current scene to the given path."""
    code = '''
    var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "' + path + '");
    Debug.Log("Saved: " + "' + path + '");
    return "saved";
    '''
    exec_csharp(code, "save scene")


def build_level3():
    """Build gamesense3.scene - 'Underground Cavern' with winding paths."""
    print("\n=== Building gamesense3.scene (Underground Cavern) ===")

    # Step 1: Create new scene
    code1 = """
    UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
    Debug.Log("Scene 3 created");
    return "created";
    """
    if not exec_csharp(code1, "create scene 3"):
        return False
    time.sleep(1)

    # Step 2: Build platforms - Section 1 (X: 0 to 50) - Ground + 1st layer
    code = """
    var cubePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Cube (1).prefab");
    var barrelPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/barrel.prefab");
    var wallPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall.prefab");
    var wallNarrowPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-narrow.prefab");
    var wallOpenPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-opening.prefab");
    var columnPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/column.prefab");
    var woodPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wood-support.prefab");
    var rocksPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/rocks.prefab");
    var gatePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/gate.prefab");
    var stairsPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/stairs.prefab");

    var cube = cubePrefab as GameObject;
    var barrel = barrelPrefab as GameObject;
    var wall = wallPrefab as GameObject;
    var wallNarrow = wallNarrowPrefab as GameObject;
    var wallOpen = wallOpenPrefab as GameObject;
    var column = columnPrefab as GameObject;
    var wood = woodPrefab as GameObject;
    var rocks = rocksPrefab as GameObject;
    var gate = gatePrefab as GameObject;
    var stairs = stairsPrefab as GameObject;

    System.Func<GameObject, float, float, float, GameObject> pl = (prefab, x, y, z) => {
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        return go;
    };

    // SECTION 1: X=0 to X=50
    // Ground platforms
    pl(cube, 0, -0.5f, 0);
    pl(cube, 2, -0.5f, 0);
    pl(cube, 4, -0.5f, 0);
    pl(cube, 6, -0.5f, 0);
    pl(cube, 8, -0.5f, 0);
    pl(cube, 10, -0.5f, 0);
    pl(cube, 12, -0.5f, 0);
    pl(cube, 14, -0.5f, 0);
    pl(cube, 16, -0.5f, 0);

    // First elevated layer at Y=2
    pl(cube, 3, 2, 0); pl(cube, 5, 2, 0); pl(cube, 7, 2, 0);
    pl(cube, 9, 2, 0); pl(cube, 11, 2, 0);

    // Second layer at Y=4
    pl(cube, 6, 4, 0); pl(cube, 8, 4, 0); pl(cube, 10, 4, 0);

    // Obstacles
    pl(barrel, 5, 0.5f, 0);
    pl(barrel, 9, 0.5f, 0);
    pl(barrel, 13, 0.5f, 0);
    pl(wall, 11, 0.5f, 0);
    pl(wallNarrow, 7, 2.5f, 0);
    pl(wallOpen, 15, 0.5f, 0);
    pl(column, 4, 0, 0);
    pl(column, 12, 0, 0);
    pl(wood, 2, -0.5f, 0);
    pl(wood, 8, -0.5f, 0);

    return "section1 done";
    """
    if not exec_csharp(code, "sec1 platforms"):
        return False

    # Step 3: Section 2 (X: 50 to 100)
    code = """
    var cubePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Cube (1).prefab");
    var barrelPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/barrel.prefab");
    var wallPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall.prefab");
    var wallNarrowPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-narrow.prefab");
    var wallOpenPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-opening.prefab");
    var columnPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/column.prefab");
    var woodPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wood-support.prefab");
    var rocksPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/rocks.prefab");
    var gatePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/gate.prefab");
    var stairsPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/stairs.prefab");

    var cube = cubePrefab as GameObject;
    var barrel = barrelPrefab as GameObject;
    var wall = wallPrefab as GameObject;
    var wallNarrow = wallNarrowPrefab as GameObject;
    var wallOpen = wallOpenPrefab as GameObject;
    var column = columnPrefab as GameObject;
    var wood = woodPrefab as GameObject;
    var rocks = rocksPrefab as GameObject;
    var gate = gatePrefab as GameObject;
    var stairs = stairsPrefab as GameObject;

    System.Func<GameObject, float, float, float, GameObject> pl = (prefab, x, y, z) => {
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        return go;
    };

    System.Func<GameObject, float, float, float, float, float, GameObject> pls = (prefab, x, y, z, sx, sy) => {
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        go.transform.localScale = new Vector3(sx, sy, 1);
        return go;
    };

    // SECTION 2: X=50 to X=100
    // Ground continues
    for (int i = 0; i <= 16; i++) {
        pl(cube, 18 + i * 2, -0.5f, 0);
    }

    // Medium layer at Y=2
    pl(cube, 20, 2, 0); pl(cube, 22, 2, 0); pl(cube, 24, 2, 0);
    pl(cube, 26, 2, 0); pl(cube, 28, 2, 0);
    pl(cube, 34, 2, 0); pl(cube, 36, 2, 0);
    pl(cube, 40, 2, 0); pl(cube, 42, 2, 0);

    // High layer at Y=5
    pl(cube, 30, 5, 0); pl(cube, 32, 5, 0);
    pl(cube, 38, 5, 0);

    // Moving platform section
    pl(cube, 48, 3, 0); pl(cube, 50, 3, 0);

    // Stairs with collision wrapper
    System.Func<float, float, string> mkStairs = (sx, sy) => {
        var parent = new GameObject("stairs_col");
        parent.transform.position = new Vector3(sx, sy, 0);
        var col = parent.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2, 2);
        col.offset = new Vector2(0, 0);

        var vis = UnityEditor.PrefabUtility.InstantiatePrefab(stairs) as GameObject;
        vis.transform.SetParent(parent.transform);
        vis.transform.localPosition = Vector3.zero;
        vis.transform.localScale = new Vector3(1, 2.2883234f, 3.4139674f);
        vis.transform.localRotation = Quaternion.Euler(0, 270, 0);
        return "stairs";
    };

    mkStairs(46, 0);
    mkStairs(52, 0.5f);

    // Obstacles
    pl(barrel, 20, 0.5f, 0); pl(barrel, 24, 0.5f, 0);
    pl(barrel, 30, -0.5f, 0); pl(barrel, 38, 0.5f, 0);
    pl(barrel, 44, 0.5f, 0); pl(barrel, 48, 3.5f, 0);
    pl(wall, 22, 0.5f, 0); pl(wall, 34, 0.5f, 0);
    pl(wallNarrow, 26, 2.5f, 0); pl(wallNarrow, 42, 2.5f, 0);
    pl(wallOpen, 18, 0.5f, 0); pl(wallOpen, 40, -0.5f, 0);
    pl(column, 28, 0, 0); pl(column, 36, 0, 0);
    pl(column, 44, 0, 0);
    pl(wood, 22, -0.5f, 0); pl(wood, 32, -0.5f, 0);
    pl(wood, 40, -0.5f, 0);
    pl(rocks, 30, 0.5f, 0); pl(rocks, 46, 0.5f, 0);
    pl(gate, 28, 0, 0); pl(gate, 50, 0, 0);

    return "section2 done";
    """
    if not exec_csharp(code, "sec2 platforms"):
        return False

    # Step 4: Section 3 (X: 100 to 160) - Most complex section
    code = """
    var cubePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Cube (1).prefab");
    var barrelPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/barrel.prefab");
    var wallPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall.prefab");
    var wallNarrowPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-narrow.prefab");
    var wallOpenPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-opening.prefab");
    var columnPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/column.prefab");
    var woodPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wood-support.prefab");
    var rocksPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/rocks.prefab");
    var gatePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/gate.prefab");
    var stairsPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/stairs.prefab");

    var cube = cubePrefab as GameObject;
    var barrel = barrelPrefab as GameObject;
    var wall = wallPrefab as GameObject;
    var wallNarrow = wallNarrowPrefab as GameObject;
    var wallOpen = wallOpenPrefab as GameObject;
    var column = columnPrefab as GameObject;
    var wood = woodPrefab as GameObject;
    var rocks = rocksPrefab as GameObject;
    var gate = gatePrefab as GameObject;
    var stairs = stairsPrefab as GameObject;

    System.Func<GameObject, float, float, float, GameObject> pl = (prefab, x, y, z) => {
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        return go;
    };

    System.Func<float, float, string> mkStairs = (sx, sy) => {
        var parent = new GameObject("stairs_col");
        parent.transform.position = new Vector3(sx, sy, 0);
        var col = parent.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2, 2);
        col.offset = new Vector2(0, 0);
        var vis = UnityEditor.PrefabUtility.InstantiatePrefab(stairs) as GameObject;
        vis.transform.SetParent(parent.transform);
        vis.transform.localPosition = Vector3.zero;
        vis.transform.localScale = new Vector3(1, 2.2883234f, 3.4139674f);
        vis.transform.localRotation = Quaternion.Euler(0, 270, 0);
        return "stairs";
    };

    // SECTION 3: X=100 to X=160 - winding multi-level
    // Ground
    for (int i = 0; i <= 14; i++) {
        pl(cube, 52 + i * 2, -0.5f, 0);
    }

    // Mid layer Y=2.5
    pl(cube, 54, 2.5f, 0); pl(cube, 56, 2.5f, 0);
    pl(cube, 58, 2.5f, 0);
    pl(cube, 64, 2.5f, 0); pl(cube, 66, 2.5f, 0); pl(cube, 68, 2.5f, 0);
    pl(cube, 74, 2.5f, 0); pl(cube, 76, 2.5f, 0);

    // Upper layer Y=5
    pl(cube, 60, 5, 0); pl(cube, 62, 5, 0);
    pl(cube, 70, 5, 0); pl(cube, 72, 5, 0);

    // Top layer Y=8
    pl(cube, 66, 8, 0); pl(cube, 68, 8, 0); pl(cube, 70, 8, 0);

    // Stairs connecting layers
    mkStairs(62, -0.5f);
    mkStairs(68, 2.5f);
    mkStairs(72, -0.5f);

    // Obstacles
    pl(barrel, 54, -0.5f, 0); pl(barrel, 60, 5.5f, 0);
    pl(barrel, 64, 3, 0); pl(barrel, 72, 0.5f, 0);
    pl(barrel, 76, 3, 0);
    pl(wall, 56, -0.5f, 0); pl(wall, 66, 3, 0);
    pl(wallNarrow, 60, -0.5f, 0); pl(wallNarrow, 74, 3, 0);
    pl(wallOpen, 58, -0.5f, 0); pl(wallOpen, 70, 5.5f, 0);
    pl(column, 54, 0, 0); pl(column, 62, 0, 0);
    pl(column, 70, 0, 0); pl(column, 76, 0, 0);
    pl(wood, 56, -0.5f, 0); pl(wood, 64, -0.5f, 0);
    pl(wood, 72, 5.5f, 0);
    pl(rocks, 58, 0.5f, 0); pl(rocks, 66, -0.5f, 0);
    pl(gate, 54, 0, 0); pl(gate, 68, 0, 0);

    return "section3 done";
    """
    if not exec_csharp(code, "sec3 platforms"):
        return False

    # Step 5: Section 4 (X: 160 to 220) + food items
    code = """
    var cubePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Cube (1).prefab");
    var barrelPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/barrel.prefab");
    var wallPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall.prefab");
    var wallNarrowPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-narrow.prefab");
    var wallOpenPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-opening.prefab");
    var columnPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/column.prefab");
    var woodPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wood-support.prefab");
    var rocksPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/rocks.prefab");
    var gatePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/gate.prefab");
    var cheesePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Cheese.prefab");
    var bananaPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Banana.prefab");
    var hamburgerPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Hamburger.prefab");
    var cherryPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Cherry.prefab");
    var waterPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Watermelon.prefab");
    var hotdogPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Hotdog.prefab");
    var olivePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Olive.prefab");
    var moneyPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Money.prefab");
    var potionPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/BluePotion.prefab");
    var heartPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Heart.prefab");
    var skullPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/SkullBones.prefab");
    var clockPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Clock.prefab");
    var bombPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Bomb.prefab");
    var firstAidPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/FirstAid.prefab");

    var cube = cubePrefab as GameObject;
    var barrel = barrelPrefab as GameObject;
    var wall = wallPrefab as GameObject;
    var wallNarrow = wallNarrowPrefab as GameObject;
    var wallOpen = wallOpenPrefab as GameObject;
    var column = columnPrefab as GameObject;
    var wood = woodPrefab as GameObject;
    var rocks = rocksPrefab as GameObject;
    var gate = gatePrefab as GameObject;
    var cheese = cheesePrefab as GameObject;
    var banana = bananaPrefab as GameObject;
    var hamburger = hamburgerPrefab as GameObject;
    var cherry = cherryPrefab as GameObject;
    var water = waterPrefab as GameObject;
    var hotdog = hotdogPrefab as GameObject;
    var olive = olivePrefab as GameObject;
    var money = moneyPrefab as GameObject;
    var potion = potionPrefab as GameObject;
    var heart = heartPrefab as GameObject;
    var skull = skullPrefab as GameObject;
    var clock = clockPrefab as GameObject;
    var bomb = bombPrefab as GameObject;
    var firstAid = firstAidPrefab as GameObject;

    System.Func<GameObject, float, float, float, GameObject> pl = (prefab, x, y, z) => {
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        return go;
    };

    // SECTION 4: X=160 to X=220 - race to the end
    // Ground
    for (int i = 0; i <= 14; i++) {
        pl(cube, 80 + i * 2, -0.5f, 0);
    }

    // Mid layer Y=2
    pl(cube, 82, 2, 0); pl(cube, 84, 2, 0);
    pl(cube, 90, 2, 0); pl(cube, 92, 2, 0);
    pl(cube, 98, 2, 0); pl(cube, 100, 2, 0);

    // Upper layer Y=4.5
    pl(cube, 86, 4.5f, 0); pl(cube, 88, 4.5f, 0);
    pl(cube, 94, 4.5f, 0); pl(cube, 96, 4.5f, 0);

    // Food on platforms
    pl(cheese, 82, 2.5f, 0); pl(cheese, 90, 2.5f, 0);
    pl(banana, 84, 2.5f, 0); pl(banana, 92, 2.5f, 0);
    pl(hamburger, 86, 5, 0); pl(hamburger, 94, 5, 0);
    pl(cherry, 88, 5, 0); pl(cherry, 100, 2.5f, 0);
    pl(water, 96, 5, 0); pl(hotdog, 98, 2.5f, 0);
    pl(olive, 80, 0, 0);

    // Items
    pl(money, 82, -0.5f, 0); pl(money, 90, -0.5f, 0);
    pl(potion, 84, -0.5f, 0); pl(potion, 92, 2.5f, 0);
    pl(heart, 86, -0.5f, 0); pl(heart, 96, -0.5f, 0);
    pl(skull, 88, -0.5f, 0); pl(clock, 94, -0.5f, 0);
    pl(bomb, 80, 0.5f, 0); pl(firstAid, 98, -0.5f, 0);

    // Obstacles
    pl(barrel, 82, 0.5f, 0); pl(barrel, 90, 0.5f, 0);
    pl(barrel, 98, 0.5f, 0);
    pl(wall, 84, -0.5f, 0); pl(wall, 94, -0.5f, 0);
    pl(wallNarrow, 88, 5, 0);
    pl(wallOpen, 86, -0.5f, 0); pl(wallOpen, 100, -0.5f, 0);
    pl(column, 80, 0, 0); pl(column, 92, 0, 0);
    pl(column, 100, 0, 0);
    pl(wood, 82, -0.5f, 0); pl(wood, 94, 2.5f, 0);
    pl(rocks, 86, 5, 0); pl(rocks, 96, 5, 0);
    pl(gate, 84, 0, 0); pl(gate, 98, 0, 0);

    return "section4 done";
    """
    if not exec_csharp(code, "sec4 platforms+food"):
        return False

    # Step 6: Section 5 (X: 220 to 280) + ending + Player + Camera + save
    code = """
    var cubePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Cube (1).prefab");
    var barrelPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/barrel.prefab");
    var wallPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall.prefab");
    var wallNarrowPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-narrow.prefab");
    var wallOpenPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-opening.prefab");
    var columnPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/column.prefab");
    var woodPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wood-support.prefab");
    var rocksPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/rocks.prefab");
    var gatePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/gate.prefab");
    var playerPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Player.prefab");
    var cheesePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Cheese.prefab");
    var moneyPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Money.prefab");

    var cube = cubePrefab as GameObject;
    var barrel = barrelPrefab as GameObject;
    var wall = wallPrefab as GameObject;
    var wallNarrow = wallNarrowPrefab as GameObject;
    var wallOpen = wallOpenPrefab as GameObject;
    var column = columnPrefab as GameObject;
    var wood = woodPrefab as GameObject;
    var rocks = rocksPrefab as GameObject;
    var gate = gatePrefab as GameObject;
    var player = playerPrefab as GameObject;
    var cheese = cheesePrefab as GameObject;
    var money = moneyPrefab as GameObject;

    System.Func<GameObject, float, float, float, GameObject> pl = (prefab, x, y, z) => {
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        return go;
    };

    // SECTION 5: X=220 to X=280 - final stretch
    // Ground
    for (int i = 0; i <= 14; i++) {
        pl(cube, 108 + i * 2, -0.5f, 0);
    }

    // Elevated platforms
    pl(cube, 110, 2, 0); pl(cube, 112, 2, 0);
    pl(cube, 118, 2.5f, 0); pl(cube, 120, 2.5f, 0);
    pl(cube, 126, 2, 0); pl(cube, 128, 2, 0);
    pl(cube, 134, 3, 0); pl(cube, 136, 3, 0);

    // High platforms
    pl(cube, 114, 5, 0); pl(cube, 116, 5, 0);
    pl(cube, 122, 6, 0); pl(cube, 124, 6, 0);
    pl(cube, 130, 5, 0); pl(cube, 132, 5, 0);

    // Final platform - goal area
    pl(cube, 138, 2, 0); pl(cube, 140, 2, 0);

    // Food and items in final section
    pl(cheese, 110, 2.5f, 0); pl(cheese, 126, 2.5f, 0);
    pl(cheese, 134, 3.5f, 0);
    pl(money, 112, -0.5f, 0); pl(money, 128, -0.5f, 0);
    pl(money, 138, 2.5f, 0);

    // Obstacles
    pl(barrel, 110, 0.5f, 0); pl(barrel, 120, 3, 0);
    pl(barrel, 130, 5.5f, 0); pl(barrel, 136, 3.5f, 0);
    pl(wall, 114, -0.5f, 0); pl(wall, 124, 6.5f, 0);
    pl(wallNarrow, 112, 2.5f, 0); pl(wallNarrow, 132, 5.5f, 0);
    pl(wallOpen, 116, -0.5f, 0); pl(wallOpen, 126, -0.5f, 0);
    pl(column, 110, 0, 0); pl(column, 120, 0, 0);
    pl(column, 130, 0, 0); pl(column, 140, 0, 0);
    pl(wood, 114, 5.5f, 0); pl(wood, 122, -0.5f, 0);
    pl(wood, 134, -0.5f, 0);
    pl(rocks, 112, -0.5f, 0); pl(rocks, 128, 2.5f, 0);
    pl(gate, 118, 0, 0); pl(gate, 136, 0, 0);

    // Place Player and Main Camera
    if (player != null) {
        var p = UnityEditor.PrefabUtility.InstantiatePrefab(player) as GameObject;
        p.transform.position = new Vector3(0, 0.5f, 0);
        p.name = "Player";
    }

    // Create main camera
    var camGO = new GameObject("Main Camera");
    camGO.tag = "MainCamera";
    var cam = camGO.AddComponent<Camera>();
    cam.orthographic = true;
    cam.orthographicSize = 7;
    cam.transform.position = new Vector3(0, 2, -10);
    camGO.AddComponent<AudioListener>();

    // Create a simple directional light
    var lightGO = new GameObject("Directional Light");
    var light = lightGO.AddComponent<Light>();
    light.type = LightType.Directional;
    light.intensity = 1.0f;
    light.transform.rotation = Quaternion.Euler(50, -30, 0);

    // Save the scene
    var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/gamesense3.scene");
    Debug.Log("Scene 3 saved!");
    return "scene3 complete";
    """
    if not exec_csharp(code, "sec5 platforms+save"):
        return False

    print(">>> gamesense3.scene built successfully!")
    return True


def build_level4():
    """Build gamesense4.scene - 'Sky Fortress' with tall open platforms."""
    print("\n=== Building gamesense4.scene (Sky Fortress) ===")

    # Create new scene
    code1 = """
    UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
    return "created";
    """
    if not exec_csharp(code1, "create scene 4"):
        return False
    time.sleep(1)

    # Build all sections
    code = """
    var cubePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Cube (1).prefab");
    var barrelPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/barrel.prefab");
    var wallPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall.prefab");
    var wallNarrowPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-narrow.prefab");
    var wallOpenPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wall-opening.prefab");
    var columnPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/column.prefab");
    var woodPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/wood-support.prefab");
    var rocksPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/rocks.prefab");
    var gatePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/gate.prefab");
    var stairsPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/stairs.prefab");
    var cheesePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Cheese.prefab");
    var bananaPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Banana.prefab");
    var hamburgerPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Hamburger.prefab");
    var cherryPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Cherry.prefab");
    var waterPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Watermelon.prefab");
    var hotdogPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Hotdog.prefab");
    var olivePrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Mg3D_Food/Olive.prefab");
    var moneyPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Money.prefab");
    var potionPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/BluePotion.prefab");
    var heartPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Heart.prefab");
    var skullPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/SkullBones.prefab");
    var clockPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Clock.prefab");
    var bombPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Bomb.prefab");
    var firstAidPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/FirstAid.prefab");
    var playerPrefab = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Prefabs/Player.prefab");

    var cube = cubePrefab as GameObject;
    var barrel = barrelPrefab as GameObject;
    var wall = wallPrefab as GameObject;
    var wallNarrow = wallNarrowPrefab as GameObject;
    var wallOpen = wallOpenPrefab as GameObject;
    var column = columnPrefab as GameObject;
    var wood = woodPrefab as GameObject;
    var rocks = rocksPrefab as GameObject;
    var gate = gatePrefab as GameObject;
    var stairs = stairsPrefab as GameObject;
    var cheese = cheesePrefab as GameObject;
    var banana = bananaPrefab as GameObject;
    var hamburger = hamburgerPrefab as GameObject;
    var cherry = cherryPrefab as GameObject;
    var water = waterPrefab as GameObject;
    var hotdog = hotdogPrefab as GameObject;
    var olive = olivePrefab as GameObject;
    var money = moneyPrefab as GameObject;
    var potion = potionPrefab as GameObject;
    var heart = heartPrefab as GameObject;
    var skull = skullPrefab as GameObject;
    var clock = clockPrefab as GameObject;
    var bomb = bombPrefab as GameObject;
    var firstAid = firstAidPrefab as GameObject;
    var player = playerPrefab as GameObject;

    System.Func<GameObject, float, float, float, GameObject> pl = (prefab, x, y, z) => {
        var go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.transform.position = new Vector3(x, y, z);
        return go;
    };

    System.Func<float, float, string> mkStairs = (sx, sy) => {
        var parent = new GameObject("stairs_col");
        parent.transform.position = new Vector3(sx, sy, 0);
        var col = parent.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2, 2);
        col.offset = new Vector2(0, 0);
        var vis = UnityEditor.PrefabUtility.InstantiatePrefab(stairs) as GameObject;
        vis.transform.SetParent(parent.transform);
        vis.transform.localPosition = Vector3.zero;
        vis.transform.localScale = new Vector3(1, 2.2883234f, 3.4139674f);
        vis.transform.localRotation = Quaternion.Euler(0, 270, 0);
        return "stairs";
    };

    // ===== SCENE 4: SKY FORTRESS =====
    // Taller platforms, more open gaps, lots of verticality

    // SECTION 1: X=0 to X=50 - grand entrance with tall columns
    // Ground level
    pl(cube, 0, -0.5f, 0); pl(cube, 2, -0.5f, 0);
    pl(cube, 4, -0.5f, 0); pl(cube, 6, -0.5f, 0);
    pl(cube, 8, -0.5f, 0); pl(cube, 10, -0.5f, 0);

    // Mid platforms Y=3 (floating islands)
    pl(cube, 4, 3, 0); pl(cube, 6, 3, 0); pl(cube, 8, 3, 0);
    pl(cube, 14, 3, 0); pl(cube, 16, 3, 0); pl(cube, 18, 3, 0);

    // Upper platforms Y=6
    pl(cube, 6, 6, 0); pl(cube, 8, 6, 0);
    pl(cube, 16, 6, 0); pl(cube, 18, 6, 0);

    // Top platform Y=9
    pl(cube, 10, 9, 0); pl(cube, 12, 9, 0); pl(cube, 14, 9, 0);

    // Stairs connecting
    mkStairs(12, -0.5f);
    mkStairs(20, 1);

    // Grand columns at entrance
    pl(column, 2, 0, 0); pl(column, 8, 0, 0);
    pl(column, 14, 0, 0); pl(column, 18, 0, 0);

    // Obstacles
    pl(barrel, 4, 0.5f, 0); pl(barrel, 12, 0.5f, 0);
    pl(wall, 6, -0.5f, 0); pl(wall, 16, -0.5f, 0);
    pl(wallNarrow, 10, 9.5f, 0);
    pl(wallOpen, 8, -0.5f, 0);
    pl(wood, 4, -0.5f, 0); pl(wood, 12, -0.5f, 0);
    pl(wood, 16, 3.5f, 0);
    pl(rocks, 6, 3.5f, 0); pl(rocks, 14, 9.5f, 0);
    pl(gate, 0, 0, 0); pl(gate, 10, 0, 0);

    // Food on high platforms
    pl(cheese, 6, 6.5f, 0); pl(cheese, 14, 9.5f, 0);
    pl(banana, 8, 6.5f, 0); pl(banana, 16, 6.5f, 0);
    pl(hamburger, 10, 9.5f, 0);
    pl(cherry, 4, 3.5f, 0); pl(cherry, 18, 3.5f, 0);
    pl(olive, 0, 0, 0);

    // Items
    pl(money, 6, -0.5f, 0); pl(money, 16, 3.5f, 0);
    pl(potion, 8, 3.5f, 0); pl(potion, 12, 9.5f, 0);
    pl(heart, 4, -0.5f, 0); pl(heart, 10, -0.5f, 0);
    pl(skull, 14, 3.5f, 0); pl(clock, 18, 6.5f, 0);
    pl(bomb, 2, 0.5f, 0); pl(firstAid, 14, -0.5f, 0);

    // ===== SECTION 2: X=50 to X=110 - bridge section =====
    // Ground
    for (int i = 0; i <= 10; i++) {
        pl(cube, 22 + i * 2, -0.5f, 0);
    }

    // Bridge platforms at Y=3
    pl(cube, 24, 3, 0); pl(cube, 26, 3, 0);
    pl(cube, 30, 3, 0); pl(cube, 32, 3, 0);
    pl(cube, 36, 3, 0); pl(cube, 38, 3, 0);

    // High platforms Y=6
    pl(cube, 28, 6, 0); pl(cube, 34, 6, 0);
    pl(cube, 40, 6, 0); pl(cube, 42, 6, 0);

    // Stairs up
    mkStairs(32, -0.5f);
    mkStairs(40, 1);

    // More obstacles
    pl(barrel, 22, 0.5f, 0); pl(barrel, 28, -0.5f, 0);
    pl(barrel, 34, -0.5f, 0); pl(barrel, 40, 0.5f, 0);
    pl(wall, 24, -0.5f, 0); pl(wall, 36, -0.5f, 0);
    pl(wallNarrow, 26, 3.5f, 0); pl(wallNarrow, 38, 3.5f, 0);
    pl(wallOpen, 30, 3.5f, 0); pl(wallOpen, 34, 6.5f, 0);
    pl(column, 24, 0, 0); pl(column, 32, 0, 0);
    pl(column, 40, 0, 0);
    pl(wood, 26, -0.5f, 0); pl(wood, 34, -0.5f, 0);
    pl(rocks, 28, 6.5f, 0); pl(rocks, 36, 3.5f, 0);
    pl(gate, 30, 0, 0); pl(gate, 38, 0, 0);

    // Food
    pl(cheese, 24, 3.5f, 0); pl(banana, 26, 3.5f, 0);
    pl(hamburger, 28, 6.5f, 0); pl(water, 34, 6.5f, 0);
    pl(hotdog, 36, 3.5f, 0); pl(cherry, 38, 3.5f, 0);
    pl(olive, 22, -0.5f, 0);

    // Items
    pl(money, 30, -0.5f, 0); pl(money, 40, 6.5f, 0);
    pl(potion, 32, 3.5f, 0); pl(heart, 42, 6.5f, 0);

    // ===== SECTION 3: X=110 to X=170 - tower climb =====
    // Ground
    for (int i = 0; i <= 10; i++) {
        pl(cube, 44 + i * 2, -0.5f, 0);
    }

    // Ascending platforms - staircase style
    pl(cube, 46, 2.5f, 0); pl(cube, 48, 2.5f, 0);
    pl(cube, 52, 5, 0); pl(cube, 54, 5, 0);
    pl(cube, 58, 7.5f, 0); pl(cube, 60, 7.5f, 0);
    pl(cube, 64, 10, 0); pl(cube, 66, 10, 0);

    // Return platforms (stepping down)
    pl(cube, 50, 4, 0); pl(cube, 56, 6.5f, 0);
    pl(cube, 62, 9, 0);

    // Ground-level islands at X=60+
    pl(cube, 60, -0.5f, 0); pl(cube, 62, -0.5f, 0);
    pl(cube, 64, -0.5f, 0); pl(cube, 66, -0.5f, 0);

    // Stairs
    mkStairs(50, -0.5f);
    mkStairs(56, 2.5f);
    mkStairs(62, 5);

    // Obstacles
    pl(barrel, 46, -0.5f, 0); pl(barrel, 52, 3, 0);
    pl(barrel, 58, 5.5f, 0); pl(barrel, 64, 8, 0);
    pl(wall, 48, -0.5f, 0); pl(wall, 54, -0.5f, 0);
    pl(wallNarrow, 50, 3, 0);
    pl(wallOpen, 56, 5, 0);
    pl(column, 46, 0, 0); pl(column, 54, 0, 0);
    pl(column, 62, 0, 0);
    pl(wood, 48, 3, 0); pl(wood, 58, -0.5f, 0);
    pl(rocks, 52, 5.5f, 0); pl(rocks, 60, 8, 0);
    pl(gate, 56, 0, 0);

    // Food
    pl(cheese, 46, 3, 0); pl(banana, 48, 3, 0);
    pl(hamburger, 52, 5.5f, 0); pl(water, 58, 8, 0);
    pl(hotdog, 60, 8, 0); pl(cherry, 64, 10.5f, 0);
    pl(olive, 44, -0.5f, 0);

    // Items
    pl(money, 50, -0.5f, 0); pl(money, 56, 5, 0);
    pl(money, 62, 7, 0);
    pl(potion, 54, 5.5f, 0); pl(potion, 66, 10.5f, 0);
    pl(heart, 58, -0.5f, 0); pl(clock, 64, -0.5f, 0);

    // ===== SECTION 4: X=170 to X=220 + ENDING =====
    // Ground
    for (int i = 0; i <= 12; i++) {
        pl(cube, 68 + i * 2, -0.5f, 0);
    }

    // Mid layer platforms
    pl(cube, 70, 2, 0); pl(cube, 72, 2, 0);
    pl(cube, 78, 2.5f, 0); pl(cube, 80, 2.5f, 0);
    pl(cube, 86, 2, 0); pl(cube, 88, 2, 0);

    // High layer
    pl(cube, 74, 5, 0); pl(cube, 76, 5, 0);
    pl(cube, 82, 6, 0); pl(cube, 84, 6, 0);

    // Final goal area
    pl(cube, 92, 2, 0); pl(cube, 94, 2, 0);

    // Stairs
    mkStairs(76, -0.5f);
    mkStairs(84, 2);
    mkStairs(90, -0.5f);

    // Obstacles
    pl(barrel, 70, 0.5f, 0); pl(barrel, 78, 3, 0);
    pl(barrel, 86, 2.5f, 0); pl(barrel, 92, 2.5f, 0);
    pl(wall, 72, -0.5f, 0); pl(wall, 80, -0.5f, 0);
    pl(wallNarrow, 74, 5.5f, 0); pl(wallNarrow, 88, 2.5f, 0);
    pl(wallOpen, 76, 5.5f, 0); pl(wallOpen, 84, 6.5f, 0);
    pl(column, 70, 0, 0); pl(column, 78, 0, 0);
    pl(column, 86, 0, 0); pl(column, 92, 0, 0);
    pl(wood, 74, -0.5f, 0); pl(wood, 82, -0.5f, 0);
    pl(wood, 88, -0.5f, 0);
    pl(rocks, 72, 2.5f, 0); pl(rocks, 80, 3, 0);
    pl(gate, 68, 0, 0); pl(gate, 90, 0, 0);

    // Food final section
    pl(cheese, 70, 2.5f, 0); pl(banana, 72, 2.5f, 0);
    pl(hamburger, 74, 5.5f, 0); pl(water, 82, 6.5f, 0);
    pl(hotdog, 84, 6.5f, 0); pl(cherry, 86, 2.5f, 0);
    pl(olive, 68, -0.5f, 0);

    // Items final section
    pl(money, 76, -0.5f, 0); pl(money, 84, -0.5f, 0);
    pl(money, 92, 2.5f, 0);
    pl(potion, 78, 3, 0); pl(potion, 88, 2.5f, 0);
    pl(heart, 80, -0.5f, 0); pl(skull, 90, -0.5f, 0);
    pl(clock, 82, -0.5f, 0); pl(bomb, 70, -0.5f, 0);

    // ===== PLAYER AND CAMERA =====
    if (player != null) {
        var p = UnityEditor.PrefabUtility.InstantiatePrefab(player) as GameObject;
        p.transform.position = new Vector3(0, 0.5f, 0);
        p.name = "Player";
    }

    var camGO = new GameObject("Main Camera");
    camGO.tag = "MainCamera";
    var cam = camGO.AddComponent<Camera>();
    cam.orthographic = true;
    cam.orthographicSize = 7;
    cam.transform.position = new Vector3(0, 2, -10);
    camGO.AddComponent<AudioListener>();

    var lightGO = new GameObject("Directional Light");
    var light = lightGO.AddComponent<Light>();
    light.type = LightType.Directional;
    light.intensity = 1.0f;
    light.transform.rotation = Quaternion.Euler(50, -30, 0);

    // Save
    var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/gamesense4.scene");
    return "scene4 complete";
    """
    if not exec_csharp(code, "full scene4"):
        return False

    print(">>> gamesense4.scene built successfully!")
    return True


def main():
    print("Starting level build for scenes 3 and 4...")
    print("Make sure Unity is running with the MCP server on port 8081!")
    print()

    # Build scene 3
    if not build_level3():
        print("ERROR: Scene 3 build failed!")
        return False

    # Build scene 4
    if not build_level4():
        print("ERROR: Scene 4 build failed!")
        return False

    print("\n=== ALL DONE ===")
    print("gamesense3.scene and gamesense4.scene created successfully!")
    return True


if __name__ == "__main__":
    main()
