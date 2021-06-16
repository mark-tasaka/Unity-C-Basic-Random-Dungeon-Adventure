using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public float chaseSpeed;
    public float alertRange;
    public Vector2 partolInterval;
    public Vector2 damageRange;

    Player player;
    LayerMask obstacleMask, walkableMask;
    Vector2 currentPosition;
    List<Vector2> availableMovementList = new List<Vector2>();
    List<Node> nodesList = new List<Node>();
    bool isMoving;
    void Start()
    {
        player = FindObjectOfType<Player>();
        obstacleMask = LayerMask.GetMask("Wall", "Enemy", "Player");
        walkableMask = LayerMask.GetMask("Wall", "Enemy");
        currentPosition = transform.position;
        StartCoroutine(Movement());


    }


    void Patrol()
    {
        availableMovementList.Clear();
        Vector2 size = Vector2.one * 0.8f;


        //Checks for Collisions
        Collider2D hitUp = Physics2D.OverlapBox(currentPosition + Vector2.up, size, 0, obstacleMask);
        if(!hitUp)
        {
            availableMovementList.Add(Vector2.up);
        }

        Collider2D hitRight = Physics2D.OverlapBox(currentPosition + Vector2.right, size, 0, obstacleMask);
        if (!hitRight)
        {
            availableMovementList.Add(Vector2.right);
        }

        Collider2D hitDown = Physics2D.OverlapBox(currentPosition + Vector2.down, size, 0, obstacleMask);
        if (!hitDown)
        {
            availableMovementList.Add(Vector2.down);
        }

        Collider2D hitLeft = Physics2D.OverlapBox(currentPosition + Vector2.left, size, 0, obstacleMask);
        if (!hitLeft)
        {
            availableMovementList.Add(Vector2.left);
        }

        if(availableMovementList.Count > 0)
        {
            int randomIndex = Random.Range(0, availableMovementList.Count);
            currentPosition += availableMovementList[randomIndex];
        }
        StartCoroutine(SmoothMove(Random.Range(partolInterval.x, partolInterval.y)));

    }

    void Attack()
    {
        int roll = Random.Range(0, 100);
        if(roll > 50)
        {
            float damangeAmount = Mathf.Ceil(Random.Range(damageRange.x, damageRange.y));
            Debug.Log(name + " attack and hit for " + damangeAmount + " points of damage.");
        }
        else
        {

            Debug.Log(name + " attack and missed. ");
        }
    }


    void CheckNode(Vector2 checkPoint, Vector2 parent)
    {
        Vector2 size = Vector2.one * 0.5f;
        Collider2D hit = Physics2D.OverlapBox(checkPoint, size, 0, walkableMask);

        if(!hit)
        {
            nodesList.Add(new Node(checkPoint, parent));
        }
    }

    Vector2 FindNextStep(Vector2 startPos, Vector2 targetPos)
    {
        int listIndex = 0;
        Vector2 myPos = startPos;
        nodesList.Clear();
        nodesList.Add(new Node(startPos, startPos));
        while(myPos != targetPos && listIndex < 1000 && nodesList.Count > 0)
        {
            //checking up, down, left & right tiles (if walkable, add to list)
            CheckNode(myPos + Vector2.up, myPos);
            CheckNode(myPos + Vector2.right, myPos);
            CheckNode(myPos + Vector2.down, myPos);
            CheckNode(myPos + Vector2.left, myPos);

            listIndex++;
            if(listIndex < nodesList.Count)
            {
                myPos = nodesList[listIndex].position;
            }
        }

        if(myPos == targetPos)
        {
            //crawl backwards through nodes list
            nodesList.Reverse();
            for(int i = 0; i < nodesList.Count; i++)
            {
                if(myPos == nodesList[i].position)
                {
                    if(nodesList[i].parent == startPos)
                    {
                        return myPos;
                    }
                    myPos = nodesList[i].parent;
                }
            }
        }
        return startPos;

    }

    IEnumerator SmoothMove(float speed)
    {
        isMoving = true;

        while(Vector2.Distance(transform.position, currentPosition) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentPosition, 5f * Time.deltaTime);
            yield return null;
        }
        transform.position = currentPosition;
        yield return new WaitForSeconds(speed);

        isMoving = false;
    }

    IEnumerator Movement()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.1f);

            if (!isMoving)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if(dist <= alertRange)
                {
                    if(dist <= 1.1f)
                    {
                        Attack();
                        yield return new WaitForSeconds(Random.Range(0.5f, 1.15f));

                    }
                    else
                    {
                        Vector2 newPosition = FindNextStep(transform.position, player.transform.position);
                        if(newPosition != currentPosition)
                        {
                            //chase
                            currentPosition = newPosition;
                            StartCoroutine(SmoothMove(chaseSpeed));
                        }
                        else
                        {
                            Patrol();
                        }
                    }
                }
                else
                {
                    Patrol();
                }
            }
        }

    }

}
