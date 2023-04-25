using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    private CollisionDetection coll;

    [Header("Stats")]
    public float speed = 10f;
    public float jumpForce = 5f;
    public float slideSpeed = 5f;

    public int side = 1;

    [Header("Booleans")]
    public bool canMove = true;
    public bool wallJumped;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CollisionDetection>();
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector2 dir = new Vector2(x, y);

        //Horizontal Movement
        Walk(dir);

        //Call Wall Slide
        if (coll.onWall && !coll.onGround)
        {
            WallSlide();
        }

        //Reset bools
        if(coll.onGround)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }

        //Jumping & Wall jumping
        if (Input.GetButtonDown("Jump"))
        {
            if(coll.onGround)
            {
                Jump(Vector2.up, false);
            }
            if(coll.onWall && !coll.onGround)
            {
                WallJump();
            }
        }
        
        //Facing direction and moving direction
        if (x > 0)
        {
            side = 1;
            //anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            //anim.Flip(side);
        }
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove) { return; }

        if(!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), .5f * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;
    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || (side == -1 && coll.onLeftWall))
        {
            side *= -1;
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;
        Debug.Log(Vector2.up / 1.5f + wallDir / 1.5f);
        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {
        rb.velocity = new Vector2(rb.velocity.x, -slideSpeed);
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
}
