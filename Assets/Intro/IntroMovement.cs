using UnityEngine;

public class IntroMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed of the player movement
    public float jumpForce = 5f;  // Force applied when the player jumps
    public float lowJumpMultiplier = 2f;  // Multiplier for downward force when jump is released early
    public float apexGravityMultiplier = 0.5f;  // Gravity multiplier at the apex of the jump
    public float apexHorizontalAcceleration = 1.5f;  // Horizontal acceleration at the apex of the jump
    public float jumpBufferTime = 0.2f;  // Time window to buffer jump input
    public float coyoteTime = 0.2f;  // Duration of coyote time
    public float maxFallSpeed = -10f;  // Maximum fall speed
    public float edgeDetectionDistance = 0.1f;  // Distance to detect edges

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool facingRight = true;  // Keep track of the player's facing direction
    private bool atApex;  // Track if the player is at the apex of their jump
    private bool canDoubleJump;  // Track if the player can perform a double jump
    private float jumpBufferCounter;  // Counter to track jump buffer time
    private float coyoteTimeCounter;  // Counter to track coyote time
    private bool nearEdge;  // Track if the player is near an edge

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Move();
        HandleJumpInput();
        ApplyJumpModifiers();
        ApplyApexModifiers();
        CheckBufferedJump();
        UpdateCoyoteTime();
        ClampFallSpeed();
        DetectEdges();
    }

    private void Move()
    {
        float moveInput = Input.GetAxis("Horizontal");

        if (!nearEdge || moveInput == 0)  // Prevent movement off the edge
        {
            float adjustedMoveSpeed = atApex ? moveSpeed * apexHorizontalAcceleration : moveSpeed;
            rb.velocity = new Vector2(moveInput * adjustedMoveSpeed, rb.velocity.y);

            if (moveInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (moveInput < 0 && facingRight)
            {
                Flip();
            }
        }
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;  // Assume the player leaves the ground upon jumping
        coyoteTimeCounter = 0;  // Reset coyote time counter
    }

    private void
        CheckBufferedJump()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
            if (isGrounded || coyoteTimeCounter > 0)
            {
                Jump();
                jumpBufferCounter = 0;
                canDoubleJump = true;  // Allow double jump after the first jump
            }
            else if (canDoubleJump && !isGrounded)
            {
                Jump();
                jumpBufferCounter = 0;
                canDoubleJump = false;  // Use up the double jump
            }
        }
    }

    private void ApplyJumpModifiers()
    {
        if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void ApplyApexModifiers()
    {
        if (!isGrounded && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            atApex = true;
            rb.gravityScale = apexGravityMultiplier;
        }
        else
        {
            atApex = false;
            rb.gravityScale = 1f;
        }
    }

    private void UpdateCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;  // Reset double jump when grounded
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void ClampFallSpeed()
    {
        if (rb.velocity.y < maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
        }
    }

    private void DetectEdges()
    {
        // Detect if the player is near the edge
        Vector2 position = transform.position;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        float distance = edgeDetectionDistance;

        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance);
        if (hit.collider == null)
        {
            nearEdge = true;
        }
        else
        {
            nearEdge = false;
        }

        Debug.DrawRay(position, direction * distance, nearEdge ? Color.red : Color.green);
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player has landed back on the ground
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Check if the player has left the ground
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = false;
        }
    }
}

