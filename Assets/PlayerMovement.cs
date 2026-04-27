using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    bool isFacingRight = true; 
    public float moveSpeed = 7f;
    public float baseSpeed = 7f;
    public float maxMoveSpeed = 18f;
    public float moveSpeedMultiplier = 2f;
    float horizontalMovement;
    public float jumpPower = 10f;
    public int maxJumps = 2;
    int jumpsRemaining; 
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    bool isWallSliding;
    bool isWallJumping;
    public float acceleration = 20f;
    public float dashPower = 10f;
    public int maxDash = 1;
    int dashRemaining; 
    bool isDashing;
    float dashTime = 0.2f;


    // Update is called once per frame
    void Update()
    {        

        GroundCheck();
        Gravity();
        wallSlide();

    if(!isWallJumping && !isDashing)
    {     

    Flip();

    float targetSpeed = horizontalMovement * baseSpeed;

    if (Mathf.Abs(horizontalMovement) > 0)
    {
        targetSpeed *= moveSpeedMultiplier;
    }

    float speedDiff = targetSpeed - rb.linearVelocity.x;
    float movement = speedDiff * acceleration * Time.deltaTime;

    rb.linearVelocity = new Vector2(
    Mathf.Clamp(rb.linearVelocity.x + movement, -maxMoveSpeed, maxMoveSpeed), rb.linearVelocity.y);
    }

        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        animator.SetBool("isWallSliding", isWallSliding);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }
    

    public void Jump(InputAction.CallbackContext context)
    {
        if(jumpsRemaining > 0)
        {
        if(context.performed)
        {
            //hold down
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            jumpsRemaining--;
            animator.SetTrigger("jump");
        }
        else if (context.canceled)
        {
            //tap 
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            jumpsRemaining--;
            animator.SetTrigger("jump");
        }
        }
    }

    private void GroundCheck()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
            dashRemaining = maxDash;
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }


    private void Gravity()
    {
        if(rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier; //who up falling faster over time
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
            isGrounded = false;
        }
    }

    private void Flip()
    {
        if(isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }

    private bool wallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    private void wallSlide()
    {
        //Not gorunded/on a wall/movement ! = 0
        if(!isGrounded && wallCheck() && horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && dashRemaining > 0 && !isDashing)
        {
        StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        dashRemaining--;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(transform.localScale.x * dashPower, 0f);
        animator.SetTrigger("dash");

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }

}