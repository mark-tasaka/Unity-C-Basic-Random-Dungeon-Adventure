using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    DungeonManager dungeonManager;

    private void Awake()
    {
        dungeonManager = FindObjectOfType<DungeonManager>();
        GameObject goFloor = Instantiate(dungeonManager.floorPrefab, transform.position, Quaternion.identity) as GameObject;
        goFloor.name = dungeonManager.floorPrefab.name;
        goFloor.transform.SetParent(dungeonManager.transform);

        if (transform.position.x > dungeonManager.maxX)
        {
            dungeonManager.maxX = transform.position.x;
        }

        if (transform.position.x < dungeonManager.minX)
        {
            dungeonManager.minX = transform.position.x;
        }


        if (transform.position.y > dungeonManager.maxY)
        {
            dungeonManager.maxY = transform.position.y;
        }

        if (transform.position.y < dungeonManager.minY)
        {
            dungeonManager.minY = transform.position.y;
        }

    }

    void Start()
    {
        LayerMask environmentMask = LayerMask.GetMask("Wall", "Floor");
        Vector2 hitSize = Vector2.one * 0.8f;

        for(int x = -1; x <= 1; x++)
        {

            for (int y = -1; y <= 1; y++)
            {
                Vector2 targetPosition = new Vector2(transform.position.x + x, transform.position.y + y);
                Collider2D hit = Physics2D.OverlapBox(targetPosition, hitSize, 0, environmentMask);
                if(!hit)
                {
                    //add wall
                    GameObject goWall = Instantiate(dungeonManager.wallPrefab, targetPosition, Quaternion.identity) as GameObject;
                    goWall.name = dungeonManager.wallPrefab.name;
                    goWall.transform.SetParent(dungeonManager.transform);

                }
            }

        }

        Destroy(gameObject);
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, Vector3.one);

        
    }

}
