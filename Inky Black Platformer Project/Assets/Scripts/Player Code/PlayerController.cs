using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    private CollisionDetection coll;

    [Header("Stats")]
    public float speed = 10f;
    public float maxSpeed = 10f;
    public float jumpForce = 5f;
    public float slideSpeed = 5f;
    public float wallJumpLerp = 10f;
    public float dashSpeed = 20f;

    public int side = 1;

    [Header("Booleans")]
    public bool canMove = true;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    public bool limitVelocity = true;

    [Space]

    private bool groundTouch;
    private bool hasDashed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CollisionDetection>();
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        //Horizontal Movement
        Walk(dir);

        //Call Wall Slide
        if (coll.onWall && !coll.onGround)
        {
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        //Reset bools
        if (coll.onGround && !isDashing)
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
        
        //Dashing
        if (Input.GetButtonDown("Fire1") && !hasDashed)
        {
            if (xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if (!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        //Limit X and Y velocity
        if (limitVelocity && !isDashing)
        {
            Vector2 clampedVelocityX = Vector2.ClampMagnitude(rb.velocity, maxSpeed);
            rb.velocity = new Vector2(clampedVelocityX.x, rb.velocity.y);
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
    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove) { return; }

        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        dir.Normalize();
        Vector2 force = dir * jumpForce;
        force.x = dir.x * speed;
        rb.velocity += force;
    }

    private void Dash(float x, float y)
    {
        hasDashed = true;
        wallJumped = true;
        rb.velocity = Vector2.zero;
        rb.velocity += new Vector2(x, y).normalized * dashSpeed;

        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        float prevGravScale = rb.gravityScale;
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        rb.gravityScale = prevGravScale;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
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
        if(!canMove) { return; }

        bool pushingWall = false;
        if ((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }

        float push = pushingWall ? 0 : rb.velocity.x;
        rb.velocity = new Vector2(push, -slideSpeed);
    }

    IEnumerator DisableMovement(float time)
    {
        limitVelocity = false;
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
        limitVelocity = true;
    }
}
