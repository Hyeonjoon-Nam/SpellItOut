/*--------------------------------------------------------------------------------*
  File Name: GenerateRooms.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GridBrushBase;

public class GenerateRooms : MonoBehaviour
{
    public GameObject enemyPrefab;
    public List<GameObject> roomPool;
    public Transform startPoint;
    public int numberOfRooms = 5;
    public GameObject exitRoom;

    private Transform lastExitDoor;
    private Serializer serializer;
    private GameObject previousSpawner;
    private int roomCount = 0;
    private enum Direction
    {
        NULL = 0,
        RIGHT,
        LEFT
    }
    private Direction lastTurn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        serializer = FindFirstObjectByType<Serializer>();

        // If there is a saved dungeon, load that
        if (GameManager.Instance.savedDungeon != null &&
            GameManager.Instance.savedDungeon.rooms.Count > 0)
        {
            serializer.LoadDungeon(GameManager.Instance.savedDungeon);
        }
        else
        {
            // first time entering dungeon
            lastExitDoor = startPoint;
            GenerateAllRooms();
        }

        // Disable everything else
        foreach (GameObject room in roomPool)
        {
            if (room) { Destroy(room); }
        }
    }

    void GenerateAllRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            SpawnRoom();
        }

        SpawnExit();
    }

    void SpawnExit()
    {
        GameObject room = Instantiate(exitRoom);

        RoomMarker rm = room.AddComponent<RoomMarker>();
        rm.prefabName = exitRoom.name;

        Transform newExit = room.transform.Find("Exit");

        // Put the new room in the next spot
        room.transform.position = lastExitDoor.position;
        room.transform.rotation = lastExitDoor.rotation;

        // Delete previous door
        Transform doorL = lastExitDoor.Find("LeftDoors");
        Transform doorR = lastExitDoor.Find("RightDoors");
        Destroy(doorL.gameObject);
        Destroy(doorR.gameObject);

        room.transform.Find("Entrance").gameObject.GetComponent<Door>().level = roomCount;

        // Save exit for next room
        lastExitDoor = newExit;
        ++roomCount;
    }

    void SpawnRoom()
    {
        // Create room
        GameObject room = SelectRoom();
        Transform newExit = room.transform.Find("Exit");

        // Put the new room in the next spot
        room.transform.position = lastExitDoor.position;
        room.transform.rotation = lastExitDoor.rotation;

        // Delete previous door
        Transform doorL = lastExitDoor.Find("LeftDoors");
        if (doorL) { Destroy(doorL.gameObject); }

        Transform doorR = lastExitDoor.Find("RightDoors");
        if (doorR) { Destroy(doorR.gameObject); }

        room.transform.Find("Entrance").gameObject.GetComponent<Door>().level = roomCount;

        // Spawn in an enemy
        SpawnEnemyAtRandomSpawner(room);

        //Transform decorTransform = room.transform.Find("Decor");
        //if (decorTransform)
        //{
        //    GameObject decorParent = decorTransform.gameObject;
        //    if (decorParent)
        //    {
        //        Destroy(decorParent);
        //    }
        //}

        //Transform wallsTransform = room.transform.Find("Walls");
        //if (wallsTransform)
        //{
        //    GameObject wallsParent = wallsTransform.gameObject;
        //    if (wallsParent)
        //    {
        //        Destroy(wallsParent);
        //    }
        //}

        // Save exit for next room
        lastExitDoor = newExit;
        ++roomCount;
    }

    GameObject SelectRoom()
    {
        GameObject prefab = roomPool[Random.Range(0, roomPool.Count)];

        // Preventing the same turn two times in a row, and preventing early exit
        if (prefab.name.StartsWith("Right"))
        {
            if (lastTurn == Direction.RIGHT) return SelectRoom();
            lastTurn = Direction.RIGHT;
        }
        if (prefab.name.StartsWith("Left"))
        {
            if (lastTurn == Direction.LEFT) return SelectRoom();
            lastTurn = Direction.LEFT;
        }
        if (prefab.name.StartsWith("Exit"))
        {
            return SelectRoom();
        }

        // instantiate
        GameObject room = Instantiate(prefab);

        // Add marker so we know what prefab this room came from
        RoomMarker rm = room.AddComponent<RoomMarker>();
        rm.prefabName = prefab.name;

        return room;
    }

    private void SpawnEnemyAtRandomSpawner(GameObject room)
    {
        // Get the parent object that holds the spawners
        Transform spawnerParent = room.transform.Find("EnemySpawners");

        if (spawnerParent == null) { return; }

        int count = spawnerParent.childCount;
        if (count == 0)
        {
            return;
        }

        // Choose a random spawner
        int index = Random.Range(0, count);
        Transform chosen = spawnerParent.GetChild(index);

        Instantiate(enemyPrefab, chosen.position, chosen.rotation);

        // Spawn enemy at the chosen spawner
        previousSpawner = chosen.gameObject;

        // Delete all other spawners
        for (int i = count - 1; i >= 0; i--)
        {
            Transform spawner = spawnerParent.GetChild(i);

            // Skip the chosen one
            if (spawner == chosen) continue;

            Destroy(spawner.gameObject);
        }
    }
}
