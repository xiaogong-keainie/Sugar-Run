using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 18f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.15f;
    public LayerMask groundLayer = ~0;

    [Header("Respawn")]
    public Vector3 startPosition = new Vector3(1f, 1f, 0f);
    public float deathY = -4f;

    [Header("Potion")]
    public int maxPotions = 3;
    public float sugarDrainPerUse = 15f;

    private int potionCount;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D selfCollider;
    private bool isGrounded;
    private float moveInput;
    private bool jumpRequested;
    private StatusUI statusUI;

    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashJump = Animator.StringToHash("Jump");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        selfCollider = GetComponent<Collider2D>();
        statusUI = FindObjectOfType<StatusUI>();
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (transform.position.y < deathY)
        {
            Respawn();
            return;
        }

        moveInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            moveInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            moveInput = 1f;

        if (moveInput != 0f)
            spriteRenderer.flipX = moveInput < 0f;

        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
            jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.Q) && potionCount > 0 && statusUI != null)
        {
            statusUI.AddSugar(-sugarDrainPerUse);
            potionCount--;
            statusUI.UpdatePotionUI(potionCount);
        }

        animator.SetFloat(HashSpeed, Mathf.Abs(moveInput));
    }

    void FixedUpdate()
    {
        isGrounded = false;

        Bounds bounds = selfCollider.bounds;
        selfCollider.enabled = false;
        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - groundCheckDistance * 0.5f);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, groundCheckDistance);
        isGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer) != null;
        selfCollider.enabled = true;

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (jumpRequested && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger(HashJump);
            jumpRequested = false;
        }
    }

    public bool AddPotion()
    {
        if (potionCount >= maxPotions) return false;
        potionCount++;
        if (statusUI != null) statusUI.UpdatePotionUI(potionCount);
        return true;
    }

    public void Respawn()
    {
        rb.velocity = Vector2.zero;
        transform.position = startPosition;

        potionCount = 0;
        var status = FindObjectOfType<StatusUI>();
        if (status != null)
        {
            status.ResetStatus();
            status.UpdatePotionUI(0);
        }
    }
}
