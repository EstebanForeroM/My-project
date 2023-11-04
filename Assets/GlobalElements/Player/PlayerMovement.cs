using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private Vector3 jumperOffset;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpRadius;
    private bool canJump;
    private bool isInJump;
    private bool isGrounded;

    [Header("Horizontal Movement Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float angleRange;
    [Range(0f, 1f)][SerializeField] private float valueLimit;
    [SerializeField] private Vector2 angleLimitOffset;

    [Header("Animations")]

    [SerializeField] Animator animator;
    [Range(0f, 1f)][SerializeField] float idleThreshold;
    [Range(0f, 1f)][SerializeField] float walkToRunThreshold;
    [Range(0, 1f)][SerializeField] float timeInJumpAnimation;
    private PlayerState currentState;

    [Header("Camera Follow")]
    [SerializeField] private Transform cameraFollow;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private float lastJumpTime;
    private float horizontalMovementAdder;
    private float horizontalMovementMultiplier;

    private bool isAlive = true;

    private void Awake()
    {
        movementInput = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        //EnhancedTouch.EnhancedTouchSupport.Enable();
        //EnhancedTouch.TouchSimulation.Enable();
        //EnhancedTouch.Touch.onFingerMove += JumpAndroid;
        InvokeRepeating(nameof(SecondUpdate), 0.1f, 0.1f);
    }

    private void OnDisable()
    {
        EnhancedTouch.Touch.onFingerMove -= JumpAndroid;
        CancelInvoke(nameof(SecondUpdate));
    }

    private void JumpAndroid(EnhancedTouch.Finger finger)
    {
        bool fingerIsOnRightSide = finger.currentTouch.screenPosition.x > Screen.width * 0.5f;
        bool fingerDeltaYIsPositive = finger.currentTouch.delta.y > 5;

        if (fingerIsOnRightSide && fingerDeltaYIsPositive)
        {
            Jump(new InputAction.CallbackContext());
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!isAlive) return;
        if (!context.performed && isGrounded && Time.time >= lastJumpTime + jumpCooldown && canJump)
        {
            lastJumpTime = Time.time;
            isInJump = true;
            ChangeAnimationState(PlayerState.jump1);
            StartCoroutine(jumpCoroutine());
        }
    }

    IEnumerator jumpCoroutine()
    {
        yield return new WaitForSeconds(timeInJumpAnimation);
        rb.AddForce(Vector2.up * jumpForce);
        yield return new WaitForSeconds(0.1f);
        isInJump = false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        float xVel = movementInput.x;
        rotatePlayerOnVelocity(xVel);
        onGroundedAnimations(xVel);
    }

    private void onGroundedAnimations(float xVel)
    {
        if (!isGrounded || isInJump) return;
        xVel = Mathf.Abs(xVel);
        if (xVel < idleThreshold) ChangeAnimationState(PlayerState.idle);
        else if (xVel < walkToRunThreshold) ChangeAnimationState(PlayerState.walk);
        else ChangeAnimationState(PlayerState.run);
    }

    private void rotatePlayerOnVelocity(float xVel)
    {
        if (xVel > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (xVel < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void SecondUpdate()
    {
        if (!isAlive) return;
        cameraFollow.position = new Vector3(transform.position.x, transform.position.y, cameraFollow.position.z);
        IsGrounded();
        GetObstacleHeight();
        onGroundedAnimations(movementInput.x);
        rb.velocity = new Vector2(movementInput.x * movementSpeed * horizontalMovementMultiplier + horizontalMovementAdder, rb.velocity.y);
    }

    private void GetObstacleHeight()
    {
        float multiplier = 1 / angleRange;
        RaycastHit2D hit2D = Physics2D.Raycast((Vector2)transform.position + angleLimitOffset, Vector2.right, angleRange, groundLayer);

        float hitDistance = hit2D.distance * multiplier;
        hitDistance = hitDistance == 0 ? 1 : hitDistance;

        if (hitDistance < valueLimit)
        {
            horizontalMovementAdder = -0.8f;
            horizontalMovementMultiplier = valueLimit / 4;
        }
        else
        {
            horizontalMovementMultiplier = hitDistance;
            horizontalMovementAdder = 0;
        }
    }

    private void IsGrounded()
    {
        Vector2 jumpDetectionPosition = transform.position + jumperOffset;
        isGrounded = Physics2D.OverlapCircle(jumpDetectionPosition, jumpRadius, groundLayer) != null;
    }

    private void OnDrawGizmosSelected()
    {
        // For obstacle height detection
        Gizmos.color = Color.green;
        Gizmos.DrawRay((Vector2)transform.position + angleLimitOffset, Vector2.right * angleRange);

        // For jump detection using OverlapCircle
        Gizmos.color = Color.red;
        Vector2 jumpDetectionPosition = transform.position + jumperOffset;
        Gizmos.DrawWireSphere(jumpDetectionPosition, jumpRadius);
    }


    private void ChangeAnimationState(PlayerState newState)
    {
        if (currentState == newState) return;

        animator.Play(newState.ToString());

        currentState = newState;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("notJumpable"))
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }

        if (other.collider.CompareTag("spikes"))
        {
            ChangeAnimationState(PlayerState.behead);
            isAlive = false;
            StartCoroutine(beheadCoroutine());
        }
    }

    IEnumerator beheadCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}

public enum PlayerState
{
    idle,
    walk,
    run,
    jump1,
    jump2,
    attack1,
    attack2,
    behead,
    push,
    pull
}
