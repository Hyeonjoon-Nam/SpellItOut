/*--------------------------------------------------------------------------------*
  File Name: Serializer.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SavedDungeon
{
    public List<SavedRoom> rooms = new List<SavedRoom>();

    public void ClearRooms()
    {
        rooms.Clear();
    }
}


[System.Serializable]
public class SavedRoom
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public int level;
}


[System.Serializable]
public class SavedEnemy
{
    public string enemyPrefabName;
    public Vector3 position;
    public float health;
}

public class Serializer : MonoBehaviour
{
    public GenerateRooms roomGenerator;
    public GameObject enemyPrefab;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public SavedDungeon SaveDungeon()
    {
        SavedDungeon sd = new SavedDungeon();

        // sort rooms by distance from origin
        RoomMarker[] rooms = FindObjectsByType<RoomMarker>(FindObjectsSortMode.None)
            .OrderBy(r => r.transform.position.sqrMagnitude)
            .ToArray();

        int numRoom = 0;
        foreach (var room in rooms)
        {
            // Room transform
            SavedRoom r = new SavedRoom();
            r.prefabName = room.prefabName;
            r.position = room.transform.position;
            r.rotation = room.transform.rotation;

            // Door level
            r.level = numRoom;

            sd.rooms.Add(r);
            ++numRoom;
        }

        return sd;
    }
    public void LoadDungeon(SavedDungeon sd)
    {
        Destroy(roomGenerator.startPoint.gameObject);
        foreach (var savedRoom in sd.rooms)
        {
            // find prefab by name from roomPool
            GameObject prefab = roomGenerator.roomPool.Find(p => p.name == savedRoom.prefabName);
            if (!prefab) { continue; }
            GameObject roomObj = Instantiate(prefab, savedRoom.position, savedRoom.rotation);

            RoomMarker rm = roomObj.AddComponent<RoomMarker>();
            rm.prefabName = savedRoom.prefabName;

            // Room/Door level
            GameObject entrance = roomObj.transform.Find("Entrance").gameObject;
            Door door = entrance.GetComponent<Door>();
            door.level = savedRoom.level;

            // Open doors that should be open
            if (savedRoom.level < gameManager.currentLevel)
            {
                Transform leftDoor = entrance.transform.Find("LeftDoors");
                leftDoor.rotation = Quaternion.Euler(
                    leftDoor.rotation.eulerAngles.x,
                    leftDoor.rotation.eulerAngles.y - 90f,
                    leftDoor.rotation.eulerAngles.z
                );

                Transform rightDoor = entrance.transform.Find("RightDoors");
                rightDoor.rotation = Quaternion.Euler(
                    rightDoor.rotation.eulerAngles.x,
                    rightDoor.rotation.eulerAngles.y + 90f,
                    rightDoor.rotation.eulerAngles.z
                );

                door.alreadyOpen = true;

                // Also the enemy would be defeated in this room too
                Destroy(roomObj.transform.Find("EnemySpawners").gameObject);
            }
            else
            {
                // Unexplored room.. select random skeleton spawner again
                Transform enemySpawners = roomObj.transform.Find("EnemySpawners");
                if (enemySpawners)
                {
                    int count = enemySpawners.childCount;
                    int index = Random.Range(0, count);
                    Transform chosen = enemySpawners.GetChild(index);
                    Instantiate(enemyPrefab, chosen.position, chosen.rotation);

                    // Delete all other spawners
                    for (int i = count - 1; i >= 0; i--)
                    {
                        Transform spawner = enemySpawners.GetChild(i);

                        // Skip the chosen one
                        if (spawner == chosen) continue;

                        Destroy(spawner.gameObject);
                    }
                }
            }

            // Destroy Exit if applicable
            Transform exitTransform = roomObj.transform.Find("Exit");
            if (exitTransform != null)
            {
                GameObject exit = exitTransform.gameObject;

                Transform rightDoorTransform = exitTransform.Find("RightDoors");
                if (rightDoorTransform != null) Destroy(rightDoorTransform.gameObject);

                Transform leftDoorTransform = exitTransform.Find("LeftDoors");
                if (leftDoorTransform != null) Destroy(leftDoorTransform.gameObject);
            }
        }
    }
}
