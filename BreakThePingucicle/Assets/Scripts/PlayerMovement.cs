using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 4f;

    private bool isFacingRight = true;
    private bool canMove = true;

    // Jump
    private int maxJumps = 2;
    private int jumpsLeft;
    [SerializeField] private float jumpingPower = 8f;
    private bool isJumping;
    private float coyoteTime = 0.02f;
    private float coyoteTimeCounter;
    private float jumpBuffer = 0.02f;
    private float jumpBufferCounter;

    // Dash
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float dashPower;
    private float dashTime = 0.1f;
    private float dashCooldown = 1f;
    private bool isAirAfterDash = false;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;

    // Artificial Gravity
    [SerializeField] private GameObject centerOfPlanet;
    [SerializeField] private float gravityForce = 9.8f;
    private Vector2 newYDir;
    private Vector2 newXDir;

    private void Start()
    {
        jumpsLeft = maxJumps;
    }

    void Update()
    {
        Vector3 gravityVect = (centerOfPlanet.transform.position - transform.position).normalized;
        rb.AddForce(gravityVect * gravityForce);

        Vector2 newYDir = (transform.position - centerOfPlanet.transform.position).normalized;
        Vector2 newXDir = new Vector2(newYDir.y, -newYDir.x);

        transform.right = newXDir;
        transform.up = newYDir;

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            if (!canMove)
                speed = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (canMove)
        {
            horizontal = Input.GetAxisRaw("Horizontal");

            if (IsGrounded() && !Input.GetButton("Jump"))
            {
                isJumping = false;
                jumpsLeft = maxJumps;
            }

            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBuffer;
                isAirAfterDash = false;
            }

            if (jumpBufferCounter > 0f)
            {
                if (!isJumping && !(coyoteTimeCounter > 0f) && jumpsLeft == maxJumps)
                {
                    jumpsLeft--;
                }
                isJumping = true;

                if ((coyoteTimeCounter > 0f) || (isJumping && jumpsLeft > 0))
                {
                    rb.velocity = newXDir * rb.velocity.x + newYDir * jumpingPower;
                    if (jumpBufferCounter == jumpBuffer)
                        jumpsLeft--;
                    jumpBufferCounter = 0f;
                }
            }

            if (Input.GetButtonUp("Jump") && Vector2.Dot(rb.velocity, newYDir) > 0f)
            {
                rb.velocity = newXDir * rb.velocity.x + newYDir * (Vector2.Dot(rb.velocity, newYDir) / 2);
                coyoteTimeCounter = 0f;
            }

            if (Input.GetButtonDown("Dash") && canDash)
            {
                StartCoroutine(Dash());
            }

            Flip();
        }
    }

    private float inertiaFactor = 0.95f; // Adjust this for how fast the player slows down (lower means faster slowdown)
    private float minVelocityThreshold = 0.1f; // Minimum velocity to consider stopping completely

    private void FixedUpdate()
    {
        Vector2 newYDir = (transform.position - centerOfPlanet.transform.position).normalized;
        Vector2 newXDir = new Vector2(newYDir.y, -newYDir.x);

        if (!isDashing)
        {
            // Regular movement along the planet's surface based on horizontal input
            if (horizontal != 0)
            {
                rb.velocity = newXDir * horizontal * speed + newYDir * Vector2.Dot(rb.velocity, newYDir); // Preserve vertical velocity
            }
            else
            {
                // When no input is given, stop horizontal movement but preserve vertical velocity
                rb.velocity = newYDir * Vector2.Dot(rb.velocity, newYDir);  // Only vertical velocity remains
            }
        }
    }


    void LateUpdate()
    {
        groundCheck.localPosition = new Vector3(0, -0.5f, 0);
        groundCheck.rotation = Quaternion.identity;
    }


    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.02f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Get mouse position in world space and calculate the direction
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dashDirection = (mousePos - transform.position).normalized;

        // Apply the dash force in the direction of the mouse along the planet's surface
        rb.velocity = dashDirection * dashPower;  // Set velocity directly

        tr.emitting = true;
        yield return new WaitForSeconds(dashTime);

        // Reduce velocity after the dash to create a post-dash inertia effect
        rb.velocity = newXDir * (rb.velocity.magnitude * 0.4f) + newYDir * Vector2.Dot(rb.velocity, newYDir);
        isAirAfterDash = true;
        tr.emitting = false;
        isDashing = false;

        // Dash cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
