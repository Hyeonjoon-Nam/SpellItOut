#if UNITY_EDITOR
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GestureAndPCGTests
{
    // ----------------------------
    // Gesture tests (EditMode)
    // ----------------------------

    [Test]
    public void Gesture_Resample_ProducesFixedCount()
    {
        // Arrange
        var pts = new List<Vector2>
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(2f, 0f),
            new Vector2(3f, 0f),
        };

        // Act
        var res = GestureMath.Resample(pts, 16);

        // Assert
        Assert.IsNotNull(res);
        Assert.AreEqual(16, res.Count);
    }

    [Test]
    public void Gesture_QuantizeDirections_RightIsZero()
    {
        // Arrange: a clean rightward stroke
        var pts = new List<Vector2>
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(2f, 0f),
            new Vector2(3f, 0f),
        };

        // Act
        var dirs = GestureMath.QuantizeDirections(pts, 8);

        // Assert
        Assert.IsNotNull(dirs);
        Assert.Greater(dirs.Count, 0);
        Assert.AreEqual(0, dirs[0]); // angle=0 => idx=0 in current implementation
    }

    [Test]
    public void Gesture_TemplateDistance_PrefersExactMatch()
    {
        // Arrange
        var input = new List<int> { 0, 0, 0, 0 };
        var exact = new int[] { 0, 0, 0, 0 };
        var wrong = new int[] { 2, 2, 2, 2 };

        // Act
        float dExact = GestureMath.DirectionSequenceDistance(input, exact, 8);
        float dWrong = GestureMath.DirectionSequenceDistance(input, wrong, 8);

        // Assert
        Assert.Less(dExact, dWrong);
        Assert.AreEqual(0f, dExact);
    }

    // ----------------------------
    // PCG tests (EditMode, invariant-based)
    // ----------------------------
    //
    // Assumption (추측입니다): Door component exists in the project, since GenerateRooms expects it.
    // If Door does not exist, this test will not compile/run until Door is available.

    [Test]
    public void PCG_GenerateRooms_SpawnsExpectedRoomMarkers()
    {
        // Arrange: deterministic RNG for repeatable generation
        Random.InitState(12345);

        // Create generator object
        var go = new GameObject("GenerateRooms_Test");
        var gen = go.AddComponent<GenerateRooms>();

        gen.numberOfRooms = 3;
        gen.roomPool = new List<GameObject>();
        gen.enemyPrefab = new GameObject("EnemyPrefab");

        // start point with dummy doors so Destroy(Left/RightDoors) won't null-ref
        var startPoint = new GameObject("StartPoint").transform;
        var spLeft = new GameObject("LeftDoors"); spLeft.transform.SetParent(startPoint);
        var spRight = new GameObject("RightDoors"); spRight.transform.SetParent(startPoint);
        gen.startPoint = startPoint;

        // Create 3 room prefabs + 1 exit prefab with required children: Entrance, Exit, EnemySpawners, LeftDoors, RightDoors
        gen.roomPool.Add(MakeRoomPrefab("StraightRoom"));
        gen.roomPool.Add(MakeRoomPrefab("RightTurnRoom"));
        gen.roomPool.Add(MakeRoomPrefab("LeftTurnRoom"));
        gen.exitRoom = MakeExitPrefab("ExitRoom");

        // Also need GameManager.Instance set, since GenerateRooms checks savedDungeon
        EnsureGameManagerSingleton();

        // Act: call Start manually in EditMode test
        gen.SendMessage("Start");

        // Assert: RoomMarker count = numberOfRooms spawned + exit
        var markers = Object.FindObjectsByType<RoomMarker>(FindObjectsSortMode.None);
        Assert.AreEqual(gen.numberOfRooms + 1, markers.Length);

        foreach (var m in markers)
        {
            Assert.IsNotNull(m);
            Assert.IsFalse(string.IsNullOrEmpty(m.prefabName));
        }
    }

    private static GameObject MakeRoomPrefab(string name)
    {
        var prefab = new GameObject(name);

        var entrance = new GameObject("Entrance"); entrance.transform.SetParent(prefab.transform);
        entrance.AddComponent<Door>(); // Assumption: exists

        var exit = new GameObject("Exit"); exit.transform.SetParent(prefab.transform);

        var enemySpawners = new GameObject("EnemySpawners"); enemySpawners.transform.SetParent(prefab.transform);
        var sp0 = new GameObject("Spawner0"); sp0.transform.SetParent(enemySpawners.transform);

        // Doors under Exit are not required for SpawnRoom, but are fine.
        var left = new GameObject("LeftDoors"); left.transform.SetParent(exit.transform);
        var right = new GameObject("RightDoors"); right.transform.SetParent(exit.transform);

        return prefab;
    }

    private static GameObject MakeExitPrefab(string name)
    {
        // Exit room uses "Exit" + "Entrance" + Door and also destroys previous doors.
        var prefab = new GameObject(name);

        var entrance = new GameObject("Entrance"); entrance.transform.SetParent(prefab.transform);
        entrance.AddComponent<Door>(); // Assumption: exists

        var exit = new GameObject("Exit"); exit.transform.SetParent(prefab.transform);
        var left = new GameObject("LeftDoors"); left.transform.SetParent(exit.transform);
        var right = new GameObject("RightDoors"); right.transform.SetParent(exit.transform);

        return prefab;
    }

    private static void EnsureGameManagerSingleton()
    {
        if (GameManager.Instance != null) return;

        var gmObj = new GameObject("GameManager_Test");
        gmObj.AddComponent<GameManager>();

        // Trigger Awake manually in EditMode
        gmObj.SendMessage("Awake");
    }
}

#endif