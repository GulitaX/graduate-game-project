using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseTerrain : MonoBehaviour
{
    [SerializeField]
    private GameObject TerrainObjectsHolder;

    //public bspTree bspTree;
    public List<GameObject> TerrainPrefabs;
    public List<GameObject> InteractionPrefabs;
    public LayerMask enviromentLayerMask;

    [Range(0f ,1f)]
    public float naturalEnviromentDensity;


    public int[,] mapedBoard;
    public float[,] terrainBoard;

    public List<Rect> RoomList;

    private void Awake()
    {
        switch (GameState.difficulty)
        {
            case GameState.difficulties.Easy:
                naturalEnviromentDensity = 0.2f;
                break;

            case GameState.difficulties.Medium:
                naturalEnviromentDensity = 0.25f;
                break;

            case GameState.difficulties.Hard:
                naturalEnviromentDensity = 0.3f;
                break;

            default:
                break;
        }
    }

    public void applyPerlinNoiseToRoom(List<Rect> roomList, List<Rect> corridorList, 
        int[,] mapBoard, int mapWidth, int mapHeight)
    {
        foreach (Rect corridor in corridorList)
        {
            if (corridor.width > corridor.height)
            {
                for (int i = (int)corridor.x - 4; i <= (int)corridor.xMax + 4; i++)
                {
                    for (int j = (int)corridor.y - 1; j <= (int)corridor.yMax + 1; j++)
                    {
                        if (i >= 0 && i < mapWidth && j >= 0 && j < mapHeight)
                        {
                            mapBoard[i, j] = 2;
                        }

                    }
                }
            }
            else if (corridor.width < corridor.height)
            {
                for (int i = (int)corridor.x - 1; i < (int)corridor.xMax + 1; i++)
                {
                    for (int j = (int)corridor.y - 4; j < (int)corridor.yMax + 4; j++)
                    {
                        if (i >= 0 && i < mapWidth && j >= 0 && j < mapHeight)
                        {
                            mapBoard[i, j] = 2;
                        }
                    }
                }
            }
        }

        this.mapedBoard = mapBoard;
        this.terrainBoard = new float[mapWidth, mapHeight];
        this.RoomList = roomList;
        foreach (Rect room in roomList)
        {
            generatePerlinNoise((int)room.x, (int)room.y, (int)room.xMax, (int)room.yMax);
        }
    }

    private void generatePerlinNoise(int start, int end, int roomWidth, int roomHeight)
    {
       
        for (int i = start; i< roomWidth; i++)
        {
            for(int j = end; j < roomHeight; j++)
            {

                float iCoord = (float)i / roomWidth * 40f + 100f;
                float jCoord = (float)j / roomHeight * 40f + 100f;

                float gradient = Mathf.PerlinNoise(iCoord, jCoord);
                terrainBoard[i, j] = gradient;
                loadPrefabByNoise(gradient, i, j);

            }
        }
    }

    private void loadPrefabByNoise(float noiseValue, int i , int j)
    {
        if (mapedBoard[i, j] == 2)
        {
            return;
        }

        else {

            if (noiseValue <= 0.25f && noiseValue > 0.2f)
            {
                enviromentSpawner(new Vector2(i, j), TerrainPrefabs[1]);
               
            }
            else if (noiseValue <= 0.35f && noiseValue > 0.3f)
            {
                enviromentSpawner(new Vector2(i, j), TerrainPrefabs[0]);
               
            }
            else if (noiseValue >= 0.5f && noiseValue <= 0.52f)
            {
                enviromentSpawner(new Vector2(i, j), InteractionPrefabs[0]);
                
            }
            else if (noiseValue > 0.6f && noiseValue <= 0.62f)
            {
                enviromentSpawner(new Vector2(i, j), InteractionPrefabs[1]);
                
            }
            else if (noiseValue > 0.8f && noiseValue < 0.85f)
            {
                enviromentSpawner(new Vector2(i, j), InteractionPrefabs[2]);
                
            }
        }
        

    }

    private void enviromentSpawner(Vector2 position, GameObject prefabToSpawn)
    {
        if(!GameObject.Find("TerrainObjectsHolder"))
        {
            TerrainObjectsHolder = new GameObject("TerrainObjectsHolder");
        }

        bool haveCollider = prefabToSpawn.GetComponent<Collider2D>();
        int isProps = 0;
        Collider2D[] colliders = new Collider2D[0];

        if (haveCollider)
        {
            Collider2D prefCollider = prefabToSpawn.GetComponent<Collider2D>();

            colliders = Physics2D.OverlapCircleAll(position, 8f, enviromentLayerMask);
        }

        //Controlling the percentage of randomly spawn organic prefabs
        if (UnityEngine.Random.Range(0f, 1f) >= Mathf.Abs(1f - naturalEnviromentDensity))
        {

            for (var i = 0; i < colliders.Length - 1; i++)
            {
                if (colliders[i].gameObject.CompareTag("Enviroments"))
                {
                    Debug.Log("found near object enviroment");
                    isProps += 1;
                }

            }
            //Ignore if there are more than 1 enviroment object with collider inside spawn radius
            if (isProps > 1)
            {
                return;
            }
            else
                GameObject.Instantiate(prefabToSpawn, position, transform.rotation, TerrainObjectsHolder.transform);
        }
    }


}
