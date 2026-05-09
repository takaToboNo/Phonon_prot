using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("基本移動")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 12.0f;
    [SerializeField] private int maxJumpCount = 2;

    [Header("接地判定 (BoxCast)")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private float groundCheckOffset = -0.1f;
    [SerializeField] private float maxSlopeAngle = 45f;

    [Header("操作感の調整")]
    [SerializeField] private float coyoteTime = 0.15f;

    [Header("バースト・エイム設定")]
    [SerializeField] private float burstForce = 25f;
    [SerializeField] private float aimTimeScale = 0.05f;
    [SerializeField] private LineRenderer aimIndicator;
    [SerializeField] private float bounceSpeedMultiplier = 1.0f;

    [Header("速度制限・演出設定")]
    [SerializeField] private float bulletModeExitThreshold = 3f; // 弾丸モードが解除される速度

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private float horizontalInput;
    private bool isGrounded;
    private float colliderHalfHeight;

    private int jumpCount = 0;
    private float coyoteTimeCounter;
    private bool isAiming = false;
    private bool isBursting = false;
    private Vector2 aimDirection;
    private Rigidbody2D movingPlatformRb;
    private Vector2 lastVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        colliderHalfHeight = col.size.y / 2f;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (aimIndicator) aimIndicator.enabled = false;

        // 残像マネージャーに自分自身を登録
        if (AfterImageManager.Instance != null)
        {
            AfterImageManager.Instance.SetPlayer(transform, GetComponent<SpriteRenderer>());
        }
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        HandleJump();
        HandleBurstInput();

        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentSpeed <= bulletModeExitThreshold)
        {
            AfterImageManager.Instance.StopEmitting();
        }
    }

    private void HandleInput()
    {
        if (Time.timeScale == 0f) { horizontalInput = 0f; return; }

        horizontalInput = 0f;
        if (Gamepad.current != null)
        {
            float stickInput = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stickInput) > 0.1f) horizontalInput = stickInput;
        }
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) horizontalInput = 1f;
            else if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
        }
    }

    private void CheckGround()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0, -colliderHalfHeight + groundCheckOffset);
        RaycastHit2D hit = Physics2D.BoxCast(origin, groundCheckSize, 0f, Vector2.down, 0.1f, groundLayer);
        if (hit.collider != null)
        {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle <= maxSlopeAngle)
            {
                isGrounded = true;
                coyoteTimeCounter = coyoteTime;
                movingPlatformRb = hit.collider.GetComponent<Rigidbody2D>();
                if (rb.linearVelocity.y <= 0.01f)
                {
                    jumpCount = 0;
                    //isBursting = false;
                }
            }
        }
        else
        {
            isGrounded = false;
            coyoteTimeCounter -= Time.deltaTime;
            movingPlatformRb = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBursting)
        {
            // ★修正：rb.linearVelocity ではなく、衝突直前の lastVelocity を使う
            // これにより、壁に当たって死んだ速度ではなく、当たる前の勢いで計算できる
            Vector2 incomingVector = lastVelocity;

            // 入射ベクトルの勢いが弱すぎる（ほぼ止まっている）場合は計算しない
            if (incomingVector.magnitude < 1f) return;

            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectDir = Vector2.Reflect(incomingVector.normalized, normal);

            // 元の速度の大きさを維持して反射
            float speed = incomingVector.magnitude;
            rb.linearVelocity = reflectDir * Mathf.Max(speed, burstForce * 0.5f) * bounceSpeedMultiplier;

            aimDirection = reflectDir.normalized;
        }
    }

    private void HandleJump()
    {
        if (isAiming) return;
        bool jumpPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                           (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (jumpPressed)
        {
            if (coyoteTimeCounter > 0f || jumpCount < maxJumpCount)
            {
                jumpCount++;
                coyoteTimeCounter = 0f;

                // 残像エフェクト停止
                if (AfterImageManager.Instance != null)
                {
                    AfterImageManager.Instance.StopEmitting();
                }

                isBursting = false;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
    }

    private void HandleBurstInput()
    {
        bool aimHeld = (Gamepad.current != null && Gamepad.current.leftTrigger.isPressed) ||
                       (Mouse.current != null && Mouse.current.leftButton.isPressed);
        if (aimHeld) StartAiming();
        else if (isAiming) ExecuteBurst();
    }

    private void StartAiming()
    {
        isAiming = true;
        isBursting = false;
        Time.timeScale = aimTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().magnitude > 0.1f)
            aimDirection = Gamepad.current.leftStick.ReadValue().normalized;
        else if (Mouse.current != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 mousePos = Mouse.current.position.ReadValue();
            aimDirection = (mousePos - screenPos).normalized;
        }
        if (aimIndicator)
        {
            aimIndicator.enabled = true;
            aimIndicator.SetPosition(0, transform.position);
            aimIndicator.SetPosition(1, (Vector2)transform.position + aimDirection * 3f);
        }
    }

    private void ExecuteBurst()
    {
        isAiming = false;
        isBursting = true;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        if (aimIndicator) aimIndicator.enabled = false;

        // ★追加：残像エフェクト開始！
        if (AfterImageManager.Instance != null)
        {
            AfterImageManager.Instance.StartEmitting();
        }

        rb.linearVelocity = aimDirection * burstForce;
    }

    void FixedUpdate()
    {
        // ★重要：衝突が起こる前の速度を毎フレーム記録しておく
        lastVelocity = rb.linearVelocity;

        if (isAiming || isBursting) return;

        Vector2 currentVel = rb.linearVelocity;
        Vector2 platformVel = (movingPlatformRb != null) ? movingPlatformRb.linearVelocity : Vector2.zero;
        float targetX = horizontalInput * moveSpeed;
        float currentXRelative = currentVel.x - platformVel.x;
        float newXRelative = Mathf.MoveTowards(currentXRelative, targetX, moveSpeed * 20f * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newXRelative + platformVel.x, currentVel.y);
    }

    private void OnDrawGizmos()
    {
        if (col == null) return;
        Vector2 origin = (Vector2)transform.position + new Vector2(0, -colliderHalfHeight + groundCheckOffset);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(origin, groundCheckSize);
    }
}