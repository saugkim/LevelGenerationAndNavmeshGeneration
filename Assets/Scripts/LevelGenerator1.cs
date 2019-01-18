using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator1 : MonoBehaviour
{

    public Room startRoomPrefab, endRoomPrefab;

    public PlayerController playerPrefab;

    public List<Room> roomPrefabs = new List<Room>();

    public Vector2 IterationRange = new Vector2(3, 10);

    List<Doorway> availableDoorways = new List<Doorway>();

    StartRoom startRoom;
    EndRoom endRoom;
    List<Room> placedRooms = new List<Room>();

    LayerMask roomLayerMask;

    PlayerController player;


    private void Start()
    {

        roomLayerMask = LayerMask.GetMask("Room");

        StartCoroutine(GenerateLevel());
    }

    IEnumerator GenerateLevel()
    {
        WaitForSeconds startUp = new WaitForSeconds(2f);
        WaitForFixedUpdate interval = new WaitForFixedUpdate();

        yield return startUp;

        PlaceStartRoom();

        yield return interval;

        int iterations = UnityEngine.Random.Range((int)IterationRange.x, (int)IterationRange.y);

        for (int i = 0; i < iterations; i++)
        {
            PlaceRoom();
            yield return interval;
        }

        PlaceEndRoom();
        yield return interval;

        Debug.Log("START GAME !! generation done");

        PlacePlayerInStartRoom();

        //   yield return new WaitForSeconds(3f);
        //   ResetLevelGenerator();
    }

    private void PlacePlayerInStartRoom()
    {
        player = Instantiate(playerPrefab) as PlayerController;
        player.transform.position = startRoom.playerStart.position;
        player.transform.rotation = startRoom.playerStart.rotation;
    }

    private void ResetLevelGenerator()
    {
        Debug.Log("Reset ");
        StopCoroutine(GenerateLevel());

        if (startRoom)
        {
            Destroy(startRoom.gameObject);
        }
        if (endRoom)
        {
            Destroy(endRoom.gameObject);
        }
        foreach (Room room in placedRooms)
        {
            Destroy(room.gameObject);
        }

        placedRooms.Clear();
        availableDoorways.Clear();

        StartCoroutine(GenerateLevel());
    }

    private void PlaceRoom()
    {
        Debug.Log("End room");
        Room currentRoom = Instantiate(roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Count)]) as Room;
        currentRoom.transform.SetParent(transform);

        List<Doorway> allAvailableDoorways = new List<Doorway>(availableDoorways);
        List<Doorway> currentRoomDoorways = new List<Doorway>();

        AddDoorwaysToList(currentRoom, ref currentRoomDoorways);

        AddDoorwaysToList(currentRoom, ref availableDoorways);

        bool roomPlaced = false;

        foreach (Doorway availableDoorway in allAvailableDoorways)
        {
            foreach (var currentDoorway in currentRoomDoorways)
            {
                PositionRoomAtDoorway(ref currentRoom, currentDoorway, availableDoorway);

                if (CheckRoomOverlap(currentRoom))
                {
                    continue;
                }

                roomPlaced = true;
                placedRooms.Add(currentRoom);

                currentDoorway.gameObject.SetActive(false);
                availableDoorways.Remove(currentDoorway);

                availableDoorway.gameObject.SetActive(false);
                availableDoorways.Remove(availableDoorway);

                break;

            }

            if (roomPlaced)
            {
                break;
            }
        }

        if (!roomPlaced)
        {
            Destroy(currentRoom.gameObject);
            ResetLevelGenerator();
        }
    }

    private bool CheckRoomOverlap(Room room)
    {
        Bounds bounds = room.RoomBounds;
        bounds.Expand(-0.1f);
        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.size / 2, room.transform.rotation, roomLayerMask);

        if (colliders.Length > 0)
        {
            foreach (var item in colliders)
            {
                if (item.transform.parent.gameObject.Equals(room.gameObject))
                {
                    continue;
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void PositionRoomAtDoorway(ref Room room, Doorway roomDoorway, Doorway targetDoorway)
    {
        room.transform.position = Vector3.zero;
        room.transform.rotation = Quaternion.identity;

        Vector3 targetDoorwayEuler = targetDoorway.transform.rotation.eulerAngles;
        Vector3 roomDoorwayEuler = roomDoorway.transform.rotation.eulerAngles;

        float deltaAngle = Mathf.DeltaAngle(roomDoorwayEuler.y, targetDoorwayEuler.y);
        Quaternion currentRoomTargetRotation = Quaternion.AngleAxis(deltaAngle, Vector3.up);

        room.transform.rotation = currentRoomTargetRotation * Quaternion.Euler(0, 180f, 0);

        Vector3 roomPositionOffset = roomDoorway.transform.position - room.transform.position;
        room.transform.position = targetDoorway.transform.position - roomPositionOffset;

    }

    private void PlaceEndRoom()
    {
        endRoom = Instantiate(endRoomPrefab) as EndRoom;
        endRoom.transform.SetParent(transform);

        List<Doorway> allAvailableDoorways = new List<Doorway>(availableDoorways);
        Doorway doorway = endRoom.doorways[0];

        bool roomPlaced = false;

        foreach (var availableDoorway in allAvailableDoorways)
        {
            Room room = (Room)endRoom;
            PositionRoomAtDoorway(ref room, doorway, availableDoorway);

            if (CheckRoomOverlap(endRoom))
            {
                continue;
            }

            roomPlaced = true;

            doorway.gameObject.SetActive(false);
            availableDoorways.Remove(doorway);

            availableDoorway.gameObject.SetActive(false);
            availableDoorways.Remove(availableDoorway);

            break;
        }

        if (!roomPlaced)
        {
            ResetLevelGenerator();
        }
    }

    private void PlaceStartRoom()
    {
        startRoom = Instantiate(startRoomPrefab) as StartRoom;
        startRoom.transform.SetParent(transform);

        AddDoorwaysToList(startRoom, ref availableDoorways);

        startRoom.transform.position = Vector3.zero;
        startRoom.transform.rotation = Quaternion.identity;

    }

    private void AddDoorwaysToList(Room room, ref List<Doorway> list)
    {
        foreach (Doorway doorway in room.doorways)
        {
            int r = UnityEngine.Random.Range(0, list.Count);
            list.Insert(r, doorway);

        }
    }
}