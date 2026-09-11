using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    List<RoomSocket> currentSockets;
    List<RoomSocket> nextSockets;
    [SerializeField] int seed;
    System.Random rng;
    [SerializeField] Room startingRoomPrefab;
    [SerializeField] List<Room> normalRoomPrefabs;
    [SerializeField] List<Room> terminalRoomPrefabs;
    [SerializeField] List<Room> connectorRoomPrefabs;
    [SerializeField] int minimumRoomCount = 10;
    int generations;
    int generatedRoomCount;
    List<Room> generatedRooms;

    private void Awake()
    {
        rng = new System.Random(seed);
        currentSockets = new List<RoomSocket>();
        nextSockets = new List<RoomSocket>();
        generatedRooms = new List<Room>();
        generations = 0;
    }

    void Start()
    {
        GenerateLevel();
    }

    void Update()
    {
        
    }

    void GenerateLevel()
    {
        generations++;

        RoomSocket currentSocket;
        generatedRoomCount = 0;

        Room startingRoom = Instantiate(startingRoomPrefab, Vector3.zero, Quaternion.identity);
        startingRoom.SwitchColliderLayer();
        ConfirmRoomBasePlacement(startingRoom);

        while(generatedRoomCount < minimumRoomCount || currentSockets.Count + nextSockets.Count > 0)
        {
            if(currentSockets.Count + nextSockets.Count == 0 && generatedRoomCount < minimumRoomCount)
            {
                RestartGeneration();
                return;
            }

            if(currentSockets.Count == 0)
            {
                currentSockets.AddRange(nextSockets);
                nextSockets.Clear();
            }

            currentSocket = currentSockets[rng.Next(0, currentSockets.Count)];

            TrySpawnAnyConnector(currentSocket, connectorRoomPrefabs);
        }
        foreach (Room room in generatedRooms) { room.PlaceRoom(); }
    }

    float GetTerminalRoomChance()
    {
        if (generatedRoomCount < minimumRoomCount)
        {
            return (currentSockets.Count + nextSockets.Count - 1f) * 0.15f;
        }
        else
        {
            return (generatedRoomCount - minimumRoomCount) * 0.1f + (currentSockets.Count + nextSockets.Count) * 0.1f;
        }
    }

    void RestartGeneration()
    {
        Debug.Log($"Generations: {generations + 1}");
        foreach(Room room in generatedRooms)
        {
            room.gameObject.SetActive(false);
            Destroy(room.gameObject);
        }
        currentSockets.Clear();
        nextSockets.Clear();
        generatedRooms.Clear();
        generatedRoomCount = 0;
        GenerateLevel();
    }

    Room TrySpawnAnyConnector(RoomSocket socket, List<Room> connectors)
    {
        List<Room> connectorsCopy = new List<Room>(connectors);
        Room testConnector;
        int randomConnectorIndex;

        List<RoomSocket> connectorSocketsCopy;
        RoomSocket testSocket;
        List<Room> connectedRooms = new List<Room>();
        Room connectedRoom = null;
        int randomConnectorSocketIndex;

        while (connectorsCopy.Count > 0)
        {
            randomConnectorIndex = rng.Next(0, connectorsCopy.Count);
            testConnector = TrySpawnRoom(socket, connectorsCopy[randomConnectorIndex]);

            connectorsCopy.RemoveAt(randomConnectorIndex);

            if (testConnector != null)
            {
                connectorSocketsCopy = new List<RoomSocket>(testConnector.Sockets);
                connectorSocketsCopy.RemoveAll(connectorSocket => connectorSocket.connectedSocket != null);
                connectedRooms.Clear();

                if (connectorSocketsCopy.Count == 0)
                {
                    testConnector.gameObject.SetActive(false);
                    Destroy(testConnector.gameObject);
                    continue;
                }

                while (connectorSocketsCopy.Count > 0)
                {
                    randomConnectorSocketIndex = rng.Next(0, connectorSocketsCopy.Count);
                    testSocket = connectorSocketsCopy[randomConnectorSocketIndex];
                    connectorSocketsCopy.RemoveAt(randomConnectorSocketIndex);

                    if (rng.NextDouble() < GetTerminalRoomChance())
                    {
                        connectedRoom = TrySpawnAnyRoom(testSocket, terminalRoomPrefabs, normalRoomPrefabs);
                    }
                    else
                    {
                        connectedRoom = TrySpawnAnyRoom(testSocket, normalRoomPrefabs);
                    }

                    if (connectedRoom == null)
                    {
                        foreach(Room generatedRoom in connectedRooms)
                        {
                            generatedRoom.gameObject.SetActive(false);
                            Destroy(generatedRoom.gameObject);
                        }
                        testConnector.gameObject.SetActive(false);
                        Destroy(testConnector.gameObject);
                        break;
                    }

                    connectedRooms.Add(connectedRoom);
                }
                if(connectedRoom != null)
                {
                    foreach(Room generatedRoom in connectedRooms)
                    {
                        ConfirmRoomBasePlacement(generatedRoom);
                    }
                    ConfirmRoomBasePlacement(testConnector);
                    currentSockets.Remove(socket);
                    return testConnector;
                }
            }
        }
        currentSockets.Remove(socket);
        return null;
    }

    Room TrySpawnAnyRoom(RoomSocket socket, List<Room> rooms, List<Room> fallbackRooms = null)
    {
        if (!socket.IsExit) { return null; }

        List<Room> roomsCopy = new List<Room>(rooms);
        Room testRoom;
        int randomRoomIndex;

        while(roomsCopy.Count > 0)
        {
            randomRoomIndex = rng.Next(0, roomsCopy.Count);
            testRoom = TrySpawnRoom(socket, roomsCopy[randomRoomIndex]);
            roomsCopy.RemoveAt(randomRoomIndex);
            if(testRoom != null)
            {
                return testRoom;
            } 
        }

        if (fallbackRooms != null)
        {
            return TrySpawnAnyRoom(socket, fallbackRooms);
        }

        return null;
    }

    Room TrySpawnRoom(RoomSocket socket, Room room)
    {
        if (!socket.IsExit) { return null; }

        RoomSocket testSocket = null;
        bool roomPlaceable = false;
        Room testRoom = Instantiate(room, Vector3.zero, Quaternion.identity);
        List<RoomSocket> testSockets = new List<RoomSocket>(testRoom.Sockets);
        while (!roomPlaceable && testSockets.Count > 0)
        {
            testSocket = testSockets[rng.Next(0, testSockets.Count)];
            testSockets.Remove(testSocket);
            if (!testSocket.IsEntrance) { continue; }
            Quaternion desiredSocketRot = socket.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
            Quaternion rotationDifference = desiredSocketRot * Quaternion.Inverse(testSocket.transform.rotation);
            testRoom.transform.rotation = rotationDifference * testRoom.transform.rotation;
            Vector3 offset = socket.transform.position - testSocket.transform.position;
            testRoom.transform.position += offset;
            Physics.SyncTransforms();
            roomPlaceable = testRoom.CheckPlacementOverlaps();
        }

        if (roomPlaceable)
        {
            testSocket.connectedSocket = socket;
            testRoom.SwitchColliderLayer();
            return testRoom;
        }
        else
        {
            testRoom.gameObject.SetActive(false);
            Destroy(testRoom.gameObject);
            return null;
        }
    }

    void ConfirmRoomBasePlacement(Room room)
    {
        List<RoomSocket> occupiedSockets = new List<RoomSocket>();
        foreach (RoomSocket socket in room.Sockets)
        {
            if(socket.connectedSocket != null)
            {
                occupiedSockets.Add(socket);
                socket.connectedSocket.connectedSocket = socket;
            }
        }
        switch (room.Type)
        {
            case RoomType.Room:
                generatedRoomCount++;
                nextSockets.AddRange(room.Sockets);
                nextSockets.RemoveAll(testSocket => occupiedSockets.Contains(testSocket));
                break;

            case RoomType.Connector:
                break;
        }
        generatedRooms.Add(room);
    }
}
