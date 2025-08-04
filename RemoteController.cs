using UnityEngine;

public class RemoteController : BasePlayerController
{
   
    private float moveSpeed = 0f;
    private int direction = 1;
    public bool isMoving = false;
    public bool isGrounded { get; private set; }

    public Transform groundCheck;
    public LayerMask groundLayer;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        groundCheck = transform.Find("GroundCheck");
        groundLayer = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        GroundCheck();
        currentState?.Update(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public void SetMovementState(int dir, bool moving, float speed)
    {
        direction = dir;
        isMoving = moving;
        moveSpeed = speed;

      
            if (isMoving)
            {
                ChangeState("Run");
            }
            else if (isGrounded)
            {
                ChangeState("Idle");
            }
        

        // Lật hướng
        transform.localScale = new Vector3(direction, 1, 1);


    }

    public void ResetState()
    {
        isMoving = false;
        ChangeState("Idle");
        rb.linearVelocity = new Vector2(0, 0);
    }

    public void SetJump(float force)
    {    
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            ChangeState("Jump",true);
        
    }

    public void SyncState(string state)
    {
        if (!IsStateLocked)
        {
            ChangeState(state);
        }
    }

    private void GroundCheck()
    {
        if (groundCheck == null) return;

        float radius = 0.15f;
        Vector2 position = groundCheck.position;
        isGrounded = Physics2D.OverlapCircle(position, radius, groundLayer);
    }
}
