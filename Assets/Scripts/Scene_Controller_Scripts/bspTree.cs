using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public class bspTree : MonoBehaviour
{
    public int boardRows, boardColumns;
    public int minRoomSize, maxRoomSize;
    public bool ApplySmooth;
    public int SmoothTime;

    public string mapSeed;
    
    [Range(0, 100)]
    public int randomFillPercent;
    public Tilemap drawFloorMap;
    public Tilemap drawWallMap;
    public TileBase floorTile;
    public TileBase corridoorTile;
    public TileBase wallTile;
    public TileBase roomTilePos;
    public int[,] boardPositionsFloor;

    public List<Rect> RoomList = new List<Rect>();
    public List<Rect> CorridorsList = new List<Rect>();
    public GameObject perlinNoise;
    public bool isCompleted = false;
    public SubDungeon rootSubDungeon;

    [System.Serializable]
    public class SubDungeon
    {
        public SubDungeon left, right;
        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0); // i.e null
        public int debugId;

        private static int debugCounter = 0;
        public List<Rect> corridors = new List<Rect>();

        public SubDungeon(Rect mrect)
        {
            this.rect = mrect;
            this.debugId = debugCounter;
            debugCounter++;
        }

        public void CreateRoom()
        {
            if (left != null)
            {
                left.CreateRoom();
            }
            if (right != null)
            {
                right.CreateRoom();
            }
            if(left != null && right != null)
            {
                CreateCorridorBetween(left, right);
            }
            if (IAmLeaf())
            {
                int roomWidth = (int)Random.Range(rect.width / 2, rect.width - 2);
                int roomHeight = (int)Random.Range(rect.height / 2, rect.height - 2);
                int roomX = (int)Random.Range(1, rect.width - roomWidth - 1);
                int roomY = (int)Random.Range(1, rect.height - roomHeight - 1);

                // room position will be absolute in the board, not relative to the sub-dungeon
                room = new Rect(rect.x + roomX, rect.y + roomY, roomWidth, roomHeight);
                Debug.Log("Created room " + room + " in sub-dungeon " + debugId + " " + rect);
                
            }
        }

        public bool IAmLeaf()
        {
            return left == null && right == null;
        }

        public bool Split(int minRoomSize, int maxRoomSize)
        {
            if (!IAmLeaf())
            {
                return false;
            }

            // choose a vertical or horizontal split depending on the proportions
            // i.e. if too wide split vertically, or too long horizontally,
            // or if nearly square choose vertical or horizontal at random
            bool splitH;
            if (rect.width / rect.height >= 1.25)
            {
                splitH = false;
            }
            else if (rect.height / rect.width >= 1.25)
            {
                splitH = true;
            }
            else
            {
                splitH = Random.Range(0.0f, 1.0f) > 0.5;
            }

            if (Mathf.Min(rect.height, rect.width) / 2 < minRoomSize)
            {
                Debug.Log("Sub-dungeon " + debugId + " will be a leaf");
                return false;
            }

            if (splitH)
            {
                // split so that the resulting sub-dungeons widths are not too small
                // (since we are splitting horizontally)
                int split = Random.Range(minRoomSize, (int)(rect.width - minRoomSize));

                left = new SubDungeon(new Rect(rect.x, rect.y, rect.width, split));
                right = new SubDungeon(
                  new Rect(rect.x, rect.y + split, rect.width, rect.height - split));
            }
            else
            {
                int split = Random.Range(minRoomSize, (int)(rect.height - minRoomSize));

                left = new SubDungeon(new Rect(rect.x, rect.y, split, rect.height));
                right = new SubDungeon(
                  new Rect(rect.x + split, rect.y, rect.width - split, rect.height));
            }

            return true;
        }

        public Rect GetRoom()
        {
            if (IAmLeaf())
            {
                return room;
            }
            if (left != null)
            {
                Rect lroom = left.GetRoom();
                if (lroom.x != -1)
                {
                    return lroom;
                }
            }
            if (right != null)
            {
                Rect rroom = right.GetRoom();
                if (rroom.x != -1)
                {
                    return rroom;
                }
            }

            // workaround non nullable structs
            return new Rect(-1, -1, 0, 0);
        }

        public void CreateCorridorBetween(SubDungeon left, SubDungeon right)
        {
            Rect lroom = left.GetRoom();
            Rect rroom = right.GetRoom();

            Debug.Log("Creating corridor(s) between " + left.debugId + "(" + lroom + ") and " + right.debugId + " (" + rroom + ")");

            // attach the corridor to a random point in each room
            Vector2 lpoint = new Vector2((int)Random.Range(lroom.x + 1, lroom.xMax - 1), (int)Random.Range(lroom.y + 1, lroom.yMax - 1));
            Vector2 rpoint = new Vector2((int)Random.Range(rroom.x + 1, rroom.xMax - 1), (int)Random.Range(rroom.y + 1, rroom.yMax - 1));

            // always be sure that left point is on the left to simplify the code
            if (lpoint.x > rpoint.x)
            {
                Vector2 temp = lpoint;
                lpoint = rpoint;
                rpoint = temp;
            }

            int w = (int)(lpoint.x - rpoint.x);
            int h = (int)(lpoint.y - rpoint.y);

            Debug.Log("lpoint: " + lpoint + ", rpoint: " + rpoint + ", w: " + w + ", h: " + h);

            // if the points are not aligned horizontally
            if (w != 0)
            {
                // choose at random to go horizontal then vertical or the opposite
                if (Random.Range(0, 1) > 0.5)
                {
                    // add a corridor to the right
                    corridors.Add(new Rect(lpoint.x, lpoint.y, Mathf.Abs(w) + 1, 5));

                    // if left point is below right point go up
                    // otherwise go down
                    if (h < 0)
                    {
                        corridors.Add(new Rect(rpoint.x, lpoint.y, 5, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(rpoint.x, lpoint.y, 5, -Mathf.Abs(h)));
                    }
                }
                else
                {
                    // go up or down
                    if (h < 0)
                    {
                        corridors.Add(new Rect(lpoint.x, lpoint.y, 5, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(lpoint.x, rpoint.y, 5, Mathf.Abs(h)));
                    }

                    // then go right
                    corridors.Add(new Rect(lpoint.x, rpoint.y, Mathf.Abs(w) + 1, 5));
                }
            }
            else
            {
                // if the points are aligned horizontally
                // go up or down depending on the positions
                if (h < 0)
                {
                    corridors.Add(new Rect((int)lpoint.x, (int)lpoint.y, 5, Mathf.Abs(h)));
                }
                else
                {
                    corridors.Add(new Rect((int)rpoint.x, (int)rpoint.y, 5, Mathf.Abs(h)));
                }
            }

        }
    }

    public void CreateBSP(SubDungeon subDungeon)
    {
        Debug.Log("Splitting sub-dungeon " + subDungeon.debugId + ": " + subDungeon.rect);
        if (subDungeon.IAmLeaf())
        {
            // if the sub-dungeon is too large
            if (subDungeon.rect.width > maxRoomSize
              || subDungeon.rect.height > maxRoomSize
              || Random.Range(0.0f, 1.0f) > 0.25)
            {

                if (subDungeon.Split(minRoomSize, maxRoomSize))
                {
                    Debug.Log("Splitted sub-dungeon " + subDungeon.debugId + " in "
                      + subDungeon.left.debugId + ": " + subDungeon.left.rect + ", "
                      + subDungeon.right.debugId + ": " + subDungeon.right.rect);

                    CreateBSP(subDungeon.left);
                    CreateBSP(subDungeon.right);
                    
                }
            }
        }
    }

    private void Awake()
    {
        switch (GameState.difficulty)
        {
            case GameState.difficulties.Easy:
                boardRows = 100;
                boardColumns = 50;
                minRoomSize = 18;
                maxRoomSize = 20;
                randomFillPercent = 20;
                break;

            case GameState.difficulties.Medium:
                boardRows = 200;
                boardColumns = 100;
                minRoomSize = 23;
                maxRoomSize = 25;
                randomFillPercent = 25;
                break;

            case GameState.difficulties.Hard:
                boardRows = 500;
                boardColumns = 200;
                minRoomSize = 25;
                maxRoomSize = 27;
                randomFillPercent = 25;
                break;

            default:
                break;
        }

    }

    void Start()
    {
       
        rootSubDungeon = new SubDungeon(new Rect(0, 0, boardRows, boardColumns));
        CreateBSP(rootSubDungeon);
        rootSubDungeon.CreateRoom();

        boardPositionsFloor = new int[boardRows, boardColumns];
        getRoomList(rootSubDungeon);
        drawFloorMap.GetComponent<Tilemap>().CompressBounds();
        drawWallMap.GetComponent<Tilemap>().CompressBounds();

        DrawCorridors(rootSubDungeon);
        DrawRooms(rootSubDungeon);
        if (ApplySmooth)
        {
            for (int i = 0; i < SmoothTime; i++)
            {
                foreach (Rect room in RoomList)
                {
                    SmoothRoom();
                }
            }
        }
        ReDraw();

        // Apply Perlin noise map to each room so I can segmented areas of enviroments
        // and use it to place enviroment prefabs to the room
        perlinNoise.GetComponent<PerlinNoiseTerrain>()
            .applyPerlinNoiseToRoom(
            roomList: RoomList,
            corridorList: CorridorsList,
            mapBoard: boardPositionsFloor,
            mapWidth: boardRows,
            mapHeight: boardColumns
            );
        
       
     
        

    isCompleted = true;
    }

    // Draw the room follow the room list which contain the rectangle area
    // that represent it position in a leaf node of BSP tree
    // Apply a random noise map inside the room so we can
    // filtered with cellular automata to get a natural separation between the wall a empty space
    private void DrawRooms(SubDungeon subDungeon)
    {

        if (subDungeon == null)
        {
            return;
        }
        if (subDungeon.IAmLeaf())
        {
            //Use the time value and hash it into integer sequence that play as a seed
            //for random noise map generation of each room
            mapSeed = Time.time.ToString();
            System.Random random = new System.Random(mapSeed.GetHashCode());

            for (int i = (int)subDungeon.room.x; i < subDungeon.room.xMax; i++)
            {
                for (int j = (int)subDungeon.room.y; j < subDungeon.room.yMax; j++)
                {

                    if (boardPositionsFloor[i,j] != 2)
                    {
                        // Pick a random integer with the the current seed and compare it with the Fill percent
                        //For this case, the higher the fill value, the more guarantee a grid position is mark as a wall
                        if (random.Next(0, 100) < randomFillPercent)
                        {
                            boardPositionsFloor[i, j] = 0;
                        }
                        else
                        {
                            boardPositionsFloor[i, j] = 1;
                        }
                    }

                    
                }
            }
        }
        else
        {
            DrawRooms(subDungeon.left);
            DrawRooms(subDungeon.right);
        }
    }


    // This function apply cellular automata which go through
    // each position in the room and decide whether it should be a wall or a floor
    private void SmoothRoom()
    {
        for (int x = 0; x < boardRows; x++)
        {
            for (int y = 0; y < boardColumns; y++)
            {
                // This is the update rule of Cellular Automata (for this case only)
                //find the position of a cell neighbor, decide if it a wall or floor based on the surounding cell value,
                // if the value is at the room edges, always mark as a wall
                if (x == 0 || x == boardRows - 1 || y == 0 || y == boardColumns - 1)
                {
                    boardPositionsFloor[x, y] = 0;
                }
                else
                {
                    // Also exclude the cell with value = 2,
                    // since I do not want to filter the path way between room
                    // (keep the path clean so player can go through easily)
                    if(boardPositionsFloor[x, y] != 2)
                    {
                        int result = getSurroundingNeighbour(x, y);
                        if (result > 4)
                        {
                            boardPositionsFloor[x, y] = 1;
                        }
                        else if (result < 4)
                        {
                            boardPositionsFloor[x, y] = 0;
                        }
                    }
                }

            }
        }
    }

    //check the neighbouring 3x3 grid to see if it is a floor,
    //exclude cells at the room edges
    private int getSurroundingNeighbour(int gridX, int gridY)
    {
        int neigbourIsFloor = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX <  boardRows && neighbourY >= 0 && neighbourY < boardColumns)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        neigbourIsFloor += boardPositionsFloor[neighbourX, neighbourY];
                    }
                }
                

            }
        }
        return neigbourIsFloor;
    }

    // Draw the pathway between room based on the list of corridor
    // and marked the cell value to 2 so we can avoid generation that might blocked the way
    private void DrawCorridors(SubDungeon subDungeon)
    {
        Vector3Int floorSize = drawFloorMap.GetComponent<Tilemap>().size;

        if (subDungeon == null || subDungeon.left == null || subDungeon.right == null)
        {
            return;
        }

        DrawCorridors(subDungeon.left);
        DrawCorridors(subDungeon.right);
        foreach (Rect corridor in subDungeon.corridors)
        {
            for (int i = (int)corridor.x; i < corridor.xMax; i++)
            {
                for (int j = (int)corridor.y; j < corridor.yMax; j++)
                {
                        drawFloorMap.SetTile(new Vector3Int(i, j, 0), corridoorTile);
                        boardPositionsFloor[i, j] = 2;

                }
            }
            CorridorsList.Add(corridor);
        }
       
    }

    private void getRoomList(SubDungeon subDungeon)
    {
        if (subDungeon == null)
        {
            return;
        }
        if (subDungeon.IAmLeaf())
        {
            RoomList.Add(subDungeon.room);
        }
        else
        {
            getRoomList(subDungeon.left);
            getRoomList(subDungeon.right);
        }
    }

    // Redrawing the after we have setup the room, wall, and apply cellular automata 
    private void ReDraw()
    {
        for (int i = 0; i < boardRows; i++)
        {
            for (int j = 0; j < boardColumns; j++)
            {
                if (boardPositionsFloor[i, j] != 2)
                {
                    if (boardPositionsFloor[i, j] == 0)
                    {
                        drawWallMap.SetTile(new Vector3Int(i, j, 0), wallTile);
                        //if(drawMap.ContainsTile() == null)
                    }
                    else if (boardPositionsFloor[i, j] == 1)
                    {
                        drawFloorMap.SetTile(new Vector3Int(i, j, 0), floorTile);
                    }
                }
               
            }
        }
    }



}

