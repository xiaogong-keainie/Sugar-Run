using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 16f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D selfCollider;
    private bool isGrounded;

    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashJump = Animator.StringToHash("Jump");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        selfCollider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        isGrounded = false;
        if (groundCheck != null)
        {
            // Temporarily disable self collider to avoid self-detection
            selfCollider.enabled = false;
            var hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
            selfCollider.enabled = true;
            isGrounded = hit.collider != null;
        }
    }

    void Update()
    {
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            moveInput = 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            moveInput = -1f;

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (moveInput != 0f)
            spriteRenderer.flipX = moveInput < 0f;

        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger(HashJump);
        }

        animator.SetFloat(HashSpeed, Mathf.Abs(moveInput));
    }
}
