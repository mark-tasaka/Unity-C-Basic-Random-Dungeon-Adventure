using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;

    LayerMask obstacleMask;
    Vector2 targetPosition;
    Transform GFX;
    float flipX;
    //bool false by default
    bool isMoving;

    void Start()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");
        GFX = GetComponentInChildren<SpriteRenderer>().transform;
        flipX = GFX.localScale.x;
        
    }

    void Update()
    {
        Move();
 

    }

    private void Move()
    {
        float horz = System.Math.Sign(Input.GetAxisRaw("Horizontal"));
        float vert = System.Math.Sign(Input.GetAxisRaw("Vertical"));

        if (Mathf.Abs(horz) > 0 || Mathf.Abs(vert) > 0)
        {
            if (Mathf.Abs(horz) > 0)
            {
                GFX.localScale = new Vector2(flipX * horz, GFX.localScale.y);

            }

            if (!isMoving)
            {
                if (Mathf.Abs(horz) > 0)
                {
                    targetPosition = new Vector2(transform.position.x + horz, transform.position.y);
                }
                else if (Mathf.Abs(vert) > 0)
                {
                    targetPosition = new Vector2(transform.position.x, transform.position.y + vert);
                }
                //check for collisons
                Vector2 hitSize = Vector2.one * 0.8f;
                Collider2D hit = Physics2D.OverlapBox(targetPosition, hitSize, 0, obstacleMask);
                if (!hit)
                {
                    StartCoroutine(SmoothMove());
                }

            }


        }

    }



    IEnumerator SmoothMove()
    {
        isMoving = true;

        //check distance between player and target position we want to move to
        while(Vector2.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            
            //loops through and skips to the next frame
            yield return null;
        }

        //corrects position after looping through
        transform.position = targetPosition;

        isMoving = false;

    }
}
