using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DungeonType { Caverns, Rooms, Winding}

public class DungeonManager : MonoBehaviour
{
    public GameObject[] randomItems, randomMonsters, roundedEdges;
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitDoorPrefab;
    [Range(50, 2000)] public int totalFloorCount;
    [Range(0,100)] public int itemSpawnPercent;
    [Range(0, 100)] public int monsterSpawnPercent;
    public bool useRoundedEdges;
    public DungeonType dungeonType;
    [Range(0, 100)] public int windingHallPercent;

    [HideInInspector] public float minX, maxX, minY, maxY;

    private List<Vector3> floorList = new List<Vector3>();

    LayerMask floorMask, wallMask;
    Vector2 hitSize;

    void Start()
    {
        hitSize = Vector2.one * 0.8f;
        floorMask = LayerMask.GetMask("Floor");
        wallMask = LayerMask.GetMask("Wall");

        switch(dungeonType)
        {
            case DungeonType.Caverns:
                RandomWalker();
                break;

            case DungeonType.Rooms:
                RoomWalker();
                break;

            case DungeonType.Winding:
                WindingWalker();
                break;
        }
    }

    private void Update()
    {
        if(Application.isEditor && Input.GetKeyDown(KeyCode.Backspace) )
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }
    }

    //create a virtual place in 3D space
    void RandomWalker()
    {
        Vector3 currentPosition = Vector3.zero;

        //set floor tile at currentPosition
        floorList.Add(currentPosition);
        while(floorList.Count < totalFloorCount)
        {
            currentPosition += RandomDirection();

            if(!InFloorList(currentPosition) )
            {
                floorList.Add(currentPosition);
            }

        }

        StartCoroutine(DelayProcess());

    }

    void RoomWalker()
    {
        Vector3 currentPosition = Vector3.zero;

        //set floor tile at currentPosition
        floorList.Add(currentPosition);
        while (floorList.Count < totalFloorCount)
        {
            currentPosition = TakeAHike(currentPosition);

            RandomRoom(currentPosition);
        }


        StartCoroutine(DelayProcess());


    }


    void WindingWalker()
    {
        Vector3 currentPosition = Vector3.zero;

        //set floor tile at currentPosition
        floorList.Add(currentPosition);
        while (floorList.Count < totalFloorCount)
        {
            currentPosition = TakeAHike(currentPosition);

            int roll = Random.Range(0, 100);

            if(roll < windingHallPercent)
            {
                RandomRoom(currentPosition);
            }


        }


        StartCoroutine(DelayProcess());


    }

    Vector3 TakeAHike(Vector3 myPosition)
    {

        Vector3 walkDir = RandomDirection();
        int walkLength = Random.Range(9, 18);

        for (int i = 0; i < walkLength; i++)
        {
            if (!InFloorList(myPosition + walkDir))
            {
                floorList.Add(myPosition + walkDir);
            }
            myPosition += walkDir;
        }

        return myPosition;
    }


    void RandomRoom(Vector3 myPosition)
    {
        //random room at the end of the long walk
        int width = Random.Range(1, 5);
        int height = Random.Range(1, 5);

        for (int w = -width; w <= width; w++)
        {
            for (int h = -height; h <= height; h++)
            {
                Vector3 offset = new Vector3(w, h, 0);
                if (!InFloorList(myPosition + offset))
                {
                    floorList.Add(myPosition + offset);
                }

            }

        }
    }


    bool InFloorList(Vector3 myPos)
    {

        for (int i = 0; i < floorList.Count; i++)
        {
            if (Vector3.Equals(myPos, floorList[i]))
            {
                return true;
            }
        }

        return false;
    }


    Vector3 RandomDirection()
    {

        switch (Random.Range(1, 5))
        {
            case 1:
                return Vector3.up;
             
            case 2:
                return Vector3.right;

            case 3:
                return Vector3.down;

            case 4:
                return Vector3.left;
        }

        return Vector3.zero;

    }

    IEnumerator DelayProcess()
    {
        for (int i = 0; i < floorList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorList[i], Quaternion.identity) as GameObject;
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform);
        }

        while (FindObjectsOfType<TileSpawner>().Length > 0)
        {
            yield return null;
        }

        //ExitDoorway();
        yield return StartCoroutine(InsetExitDoorway());

        for (int x = (int)minX - 2; x <= (int)maxX + 2; x++)
        {
            for (int y = (int)minY - 2; y <= (int)maxY + 2; y++)
            {
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask);
                if (hitFloor)
                {
                    //makes sure not the position of the exit doorway
                    if (!Vector2.Equals(hitFloor.transform.position, floorList[floorList.Count - 1]))
                    {
                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);

                        RandomItems(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                        RandomMonsters(hitFloor, hitTop, hitRight, hitBottom, hitLeft);

                    }
                }

                RoundedEdges(x, y);

            }

        }
    }

    void RoundedEdges(int x, int y)
    {
        if(useRoundedEdges)
        {
            Collider2D hitWall = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, wallMask);

            if (hitWall)
            {
                Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);
                int bitValue = 0;

                if (!hitTop)
                {
                    bitValue += 1;
                }

                if (!hitRight)
                {
                    bitValue += 2;
                }

                if (!hitBottom)
                {
                    bitValue += 4;
                }

                if (!hitLeft)
                {
                    bitValue += 8;
                }

                if (bitValue > 0)
                {
                    GameObject goEdge = Instantiate(roundedEdges[bitValue], new Vector2(x, y), Quaternion.identity) as GameObject;
                    goEdge.name = roundedEdges[bitValue].name;
                    goEdge.transform.SetParent(hitWall.transform);
                }
            }
        }

    }

    void RandomMonsters(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        if(!hitTop && !hitRight && !hitBottom && !hitLeft)
        {
            int roll = Random.Range(1, 101);
            if (roll <= monsterSpawnPercent)
            {
                int monsterIndex = Random.Range(0, randomMonsters.Length);
                GameObject goMonster = Instantiate(randomMonsters[monsterIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goMonster.name = randomMonsters[monsterIndex].name;
                goMonster.transform.SetParent(hitFloor.transform);

            }

        }
    }

    void RandomItems(Collider2D hitFloor, Collider2D hitTop, Collider2D hitRight, Collider2D hitBottom, Collider2D hitLeft)
    {
        //To see if it is okay to add an item
        if ((hitTop || hitRight || hitBottom || hitLeft) && !(hitTop && hitBottom) && !(hitLeft && hitRight))
        {
            int roll = Random.Range(1, 101);
            if (roll <= itemSpawnPercent)
            {
                int itemIndex = Random.Range(0, randomItems.Length);
                GameObject goItem = Instantiate(randomItems[itemIndex], hitFloor.transform.position, Quaternion.identity) as GameObject;
                goItem.name = randomItems[itemIndex].name;
                goItem.transform.SetParent(hitFloor.transform);

            }

        }

    }

    void ExitDoorway()
    {
        Vector3 doorPosition = floorList[floorList.Count - 1];

        GameObject goDoor = Instantiate(exitDoorPrefab, doorPosition, Quaternion.identity) as GameObject;
        goDoor.name = exitDoorPrefab.name;
        goDoor.transform.SetParent(transform);
    } 

    IEnumerator InsetExitDoorway()
    {
        Vector3 walkDirection = RandomDirection();
        bool isExitPlaced = CheckExitCondition(floorList[floorList.Count-1]);

        while(!isExitPlaced)
        {
            Vector3 currentPos = WalkStraight(walkDirection);
            yield return null;
            isExitPlaced = CheckExitCondition(currentPos);
        }
        yield return null;
    }

    bool CheckExitCondition(Vector2 currentPos)
    {
        int numWalls = 0;
        
        if(Physics2D.OverlapBox(currentPos + Vector2.up, hitSize, 0, wallMask) )
        {
            numWalls++;
        }

        if (Physics2D.OverlapBox(currentPos + Vector2.right, hitSize, 0, wallMask))
        {
            numWalls++;
        }

        if (Physics2D.OverlapBox(currentPos + Vector2.down, hitSize, 0, wallMask))
        {
            numWalls++;
        }

        if (Physics2D.OverlapBox(currentPos + Vector2.left, hitSize, 0, wallMask))
        {
            numWalls++;
        }

        if(numWalls == 3)
        {
            ExitDoorway();
            return true;
        }

        return false;
    }


    Vector3 WalkStraight(Vector3 walkDir)
    {
        Vector3 myPos = floorList[floorList.Count - 1] + walkDir;
        if(InFloorList(myPos) )
        {
            floorList.Remove(myPos);
        }
        else
        {
            Collider2D hitWall = Physics2D.OverlapBox(myPos, hitSize, 0, wallMask);
            if (hitWall)
            {
                DestroyImmediate(hitWall.gameObject);
            }
                
            GameObject goTile = Instantiate(tilePrefab, myPos, Quaternion.identity, transform) as GameObject;
            goTile.name = tilePrefab.name;
            
        }
        floorList.Add(myPos);
        return myPos;
    }
}
