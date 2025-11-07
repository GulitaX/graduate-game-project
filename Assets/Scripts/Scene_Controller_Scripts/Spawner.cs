using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject MonsterObjectsHolder;

    public bspTree bspManager;
    public PerlinNoiseTerrain noiseTerrain;
    private float[,] terrainMap;
    public List<Rect> roomList;
    private int[,] mapboard;
    public LayerMask terrainLayerMask;
    public GameObject teleporter;
    public GameObject playerPrefab;
    public GameObject interactablePoint;
    public Dictionary<char , List<int>> groupedRooms;
    private List<int> roomAreas;


    public Vector2Int MinMaxOfSmallRoom;
    public Vector2Int MinMaxOfMediumRoom;
    public Vector2Int MinMaxOfLargeRoom;

    public List<GameObject> monsterPrefab;

    IEnumerator Start()
    {

        // Wait for the dungeon and environments to be completely generated
        yield return new WaitUntil(() => bspManager.isCompleted);

        roomList = bspManager.RoomList;
        mapboard = bspManager.boardPositionsFloor;
        terrainMap = noiseTerrain.terrainBoard;

        changeSpawnNumberOnDifficulty();
        monsterSpawner();

    }

    public void monsterSpawner()
    {
        groupedRooms = groupByRoomSize();
        print("Small rooms: " + string.Join(", ", groupedRooms['S']));
        print("Medium rooms: " + string.Join(", ", groupedRooms['M']));
        print("Large rooms: " + string.Join(", ", groupedRooms['L']));

        foreach (var room in roomList)
        {
            Vector2 pos = room.position;
            int middleX = (int)(room.width / 2 + pos.x);
            int middleY = (int)(room.height / 2 + pos.y);
            if (roomList.IndexOf(room) != 0)
            {
                if (groupedRooms['S'].Contains((int)room.width * (int)room.height))
                {
                    spawnByRoomSize(room, MinMaxOfSmallRoom.x, MinMaxOfSmallRoom.y, 0);
                }
                else if (groupedRooms['M'].Contains((int)room.width * (int)room.height))
                {
                    spawnByRoomSize(room, MinMaxOfMediumRoom.x, MinMaxOfMediumRoom.y, 0);
                }
                else if (groupedRooms['L'].Contains((int)room.width * (int)room.height))
                {
                    spawnByRoomSize(room, MinMaxOfLargeRoom.x, MinMaxOfLargeRoom.y, 0);
                }

            }

            // Spawn a check point at the middle of the final room
            if(roomList.IndexOf(room) == roomList.Count - 1)  
            {
                Vector2 space = findFreeSpace(new Vector2(middleX, middleY), 5f);
                GameObject exitPoint = GameObject.Instantiate(teleporter, new Vector3Int(middleX, middleY, 0), transform.rotation);
                GameObject interaction = GameObject.Find("Exit Interaction");
                interaction.transform.position = exitPoint.transform.position;
                interaction.transform.parent = exitPoint.transform;

                interaction.GetComponent<CircleCollider2D>().radius = 4f;
                interaction.GetComponent<InteractablePoint>().interactKey = KeyCode.E;

            }
            // Spawn a check point at the middle of the first room
            // and find empty area to spawn the player 
            if (roomList.IndexOf(room) == 0)
            {
                GameObject teleporterPos = GameObject.Instantiate(teleporter, new Vector3Int(middleX, middleY, 0), transform.rotation);
                Camera camera = GameObject.FindFirstObjectByType<Camera>();
                
                Vector2 space = findFreeSpace(teleporterPos.transform.position, 5f);
                if(space == Vector2Int.zero)
                {
                    playerPrefab.transform.position = new Vector3(middleX, middleY, 0);
                    print("Available space found:" + space);
                }
                else
                {
                    playerPrefab.transform.position = new Vector3(space.x, space.y, 0);
                    print("Available space found:" + space);
                }
                camera.transform.position = playerPrefab.transform.position;
            }
        }

    }

    private Vector2 findFreeSpace(Vector2 position, float checkRadius)
    {
        
        Collider2D[] enviromentCollider = Physics2D.OverlapCircleAll(new Vector2Int ((int) position.x, (int) position.y), checkRadius, terrainLayerMask);

        List<Vector3> crowdedSpace = new List<Vector3>();
        foreach(Collider2D c in enviromentCollider)
        {
            crowdedSpace.Add(c.gameObject.transform.position);
        }

        for (var i = 0; i < enviromentCollider.Length - 1; i++)
        {
            if (enviromentCollider[i].gameObject.CompareTag("Enviroments"))
            {
                Debug.Log("found near object enviroment");
                float distance = Vector3.Distance(position, enviromentCollider[i].gameObject.transform.position);

                if(distance < checkRadius - 1f)
                {
                    if (distance <= checkRadius - 3f && enviromentCollider[i].gameObject.name != "Wizard_Statue(Clone)")
                    {
                        Destroy(enviromentCollider[i].gameObject);
                        return position;
                    }
                    return Vector2.zero;
                }
            }
        }

        return position;
    }

    private void spawnByRoomSize(Rect room, int minMonster, int maxMonster, int tried)
    {
        int trial = tried;
        int totalSpawned = 0;

        if (trial > 3)
        {
            Debug.LogError("Room " + roomList.IndexOf(room) + " reaches max tried");
            return;
        }

        for (int i = (int)room.x +4; i < (int)room.xMax - 4; i+=2)
        {
            for (int j = (int)room.y +4; j < (int)room.yMax - 4; j+=2)
            {
                if (totalSpawned >= maxMonster)
                {
                    Debug.LogError("Room " + roomList.IndexOf(room) + " reaches max number of monster");
                    return;
                }

                float monsterRate = Mathf.PerlinNoise(i / room.width * 10f + 100f, j / room.height * 10f + 100f);

                if (monsterRate > 0.6f && monsterRate <= 0.7f)
                {
                    if(spawnMonster(new Vector2(i, j), monsterPrefab[0]))
                    {
                        totalSpawned++;
                    }

                }
                else if(monsterRate > 0.7f && monsterRate <= 0.78f && 
                    groupedRooms['L'].Contains((int)room.width * (int)room.height))
                {
                    if (spawnMonster(new Vector2(i, j), monsterPrefab[1]))
                    {
                        totalSpawned++;
                    }
                    
                }

                if(GameState.difficulty != GameState.difficulties.Easy)
                {
                    if (monsterRate > 0.88f && monsterRate <= 1f &&
                    (groupedRooms['L'].Contains((int)room.width * (int)room.height) ||
                    groupedRooms['M'].Contains((int)room.width * (int)room.height)))
                    {
                        if (spawnMonster(new Vector2(i, j), monsterPrefab[2]))
                        {
                            totalSpawned++;
                        }
                    }
                }

            }
        }
        
        if (totalSpawned < minMonster)
        {
            trial++;
            Debug.LogError("Room " + roomList.IndexOf(room) + " tried to spawn again, tried: " + trial);
            spawnByRoomSize(room, minMonster, maxMonster, trial);
        }

    }

    // Group rooms based on the calculated threshold
    private Dictionary<char, List<int>> groupByRoomSize()
    {
        List<int> smallRooms = new List<int>();
        List<int> mediumRooms = new List<int>();
        List<int> largeRooms = new List<int>();

        roomAreas = new List<int>();
        foreach (Rect room in roomList)
        {
            roomAreas.Add((int)room.width * (int)room.height);
        }

        Vector2Int threshold = defineRoomThreshold(roomAreas);
        int lowThreshold = threshold.x; 
        int highThreshold = threshold.y;

        foreach (int room in roomAreas)
        {
            // Check the size of the room
            if (room < lowThreshold)
            {
                smallRooms.Add(room);
            }
            else if (room >= lowThreshold && room < highThreshold)
            {
                mediumRooms.Add(room);
            }
            else
            {
                largeRooms.Add(room);
            }
        }
        return new Dictionary<char, List<int>>()
            {
                { 'S', smallRooms },
                { 'M', mediumRooms },
                { 'L', largeRooms }
            };
    }

    // Calculate the low and high threshold
    // based on the min, max, and mean of room areas list
    private Vector2Int defineRoomThreshold(List<int> roomAreas)
    {
        int min = roomAreas.Min();
        int max = roomAreas.Max();
        int mean = (int)roomAreas.Average();

        Vector2Int threshold = new Vector2Int(min + (mean - min) / 2, mean + (max - mean) / 2);

        return threshold;
    }

    private bool spawnMonster(Vector2 pos, GameObject monsterPrefab )
    {
        if(!GameObject.Find("MonsterObjectsHolder"))
        {
            MonsterObjectsHolder = new GameObject("MonsterObjectsHolder");
        }

        Vector2 spawnpoint = new Vector2(Random.Range(pos.x - 2f, pos.x + 2f), Random.Range(pos.y - 2f, pos.y + 2f));
        Vector2 space = findFreeSpace(spawnpoint, 5f);

        if (space != Vector2Int.zero && mapboard[(int)space.x, (int)space.y] != 0)
        {
            print("spawned skeleton");
            GameObject.Instantiate(monsterPrefab, new Vector3(space.x, space.y, 0), transform.rotation, MonsterObjectsHolder.transform);

            return true;

        }
        else return false;
    }

    private void changeSpawnNumberOnDifficulty()
    {
        switch (GameState.difficulty)
        {
            case GameState.difficulties.Easy:
                Vector2Int lower2Easy = new Vector2Int(-MinMaxOfSmallRoom.x, -3);
                MinMaxOfSmallRoom += lower2Easy;
                MinMaxOfMediumRoom += lower2Easy;
                MinMaxOfLargeRoom += lower2Easy;
                break;
            case GameState.difficulties.Medium:
                Vector2Int lower2Medium = new Vector2Int(-2, -2);
                MinMaxOfSmallRoom += lower2Medium;
                MinMaxOfMediumRoom += lower2Medium;
                MinMaxOfLargeRoom += lower2Medium;
                break;
            case GameState.difficulties.Hard:
                
                break;


        }
    }

    public void makeExitPoint()
    {
        Debug.Log("To pause menu");
        GameObject menuCanvas = GameObject.Find("Menu Canvas");
        menuCanvas.transform.Find("Pause Button").gameObject.SetActive(false);
        MenuScripts Menu = GameObject.FindFirstObjectByType<MenuScripts>();
        Menu.GameOver(true);
    }
}

 
