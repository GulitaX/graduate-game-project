using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class SaveManager : MonoBehaviour
{

    // Apply Singleton to implement a Save and Load system
    // for the game map (still in developement)

    public static SaveManager instance;

    string treeFilePath;
    string mapFilePath;

    private void Awake()
    {
        treeFilePath = Application.dataPath + "/treeData.txt";
        mapFilePath = Application.dataPath + "/mapData.txt";

        if(!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SaveGame(bspTree bsp)
    {
        MapData mapData = new MapData();
        int[,] mapBoard = bsp.boardPositionsFloor;
        mapData.tileDatas = new List<TileData>();

        for(int i = 0; i < mapBoard.GetLength(0); i++)
        {
            for(int j = 0; j < mapBoard.GetLength(1); j++)
            {
                TileData tile = new TileData();
                tile.position = new Vector3Int(i, j, 0);
                tile.value = mapBoard[i, j];
                mapData.tileDatas.Add(tile);
            }
        }

        string jsonBsp = JsonUtility.ToJson(bsp);
        string jsonDungeonBoard = JsonUtility.ToJson(mapData);

        Debug.Log("map data of bspTree: " + jsonBsp);
        Debug.Log("map data of board: " + jsonDungeonBoard);

        File.WriteAllText(treeFilePath, jsonBsp);
        File.WriteAllText(mapFilePath, jsonDungeonBoard);

    }

    public bspTree Loadgame()
    {
        if (File.Exists(treeFilePath))
        {
            string jsonString = File.ReadAllText(treeFilePath);
            Debug.Log("Data: " + jsonString);

            bspTree bsp = JsonUtility.FromJson<bspTree>(jsonString);
            //Debug.Log("Returned data: " + dungeonData.mapWidth + "/" + dungeonData.mapHeight);

            return bsp;
        } 
        else
        {
            Debug.LogError("Save file is missing in " + treeFilePath);
            return null;
        }
    }
}

[System.Serializable]
public class MapData
{
    public List<TileData> tileDatas;
    
}

[System.Serializable]
public class TileData
{
    public Vector3Int position;
    public int value;
}

[System.Serializable]
public class EnviromentData
{
    public Vector3 position;

}
