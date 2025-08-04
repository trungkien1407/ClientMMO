using UnityEngine;
using System.Collections;

public class PlayerController : BasePlayerController
{
   public PlayerMovement playerMovement;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public bool IsRunningInput { get; private set; }
    public bool IsGroundedLocal { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }
    protected void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        groundCheckPoint = playerMovement.groundCheck;
        groundLayer = playerMovement.groundLayer;

    }

    private void Update()
    {
        IsGroundedLocal = playerMovement.IsGrounded;
    
        // Chỉ chặn logic chuyển state khi bị lock, nhưng vẫn update animation hiện tại
        if (!IsStateLocked)
        {
            // Kiểm tra mặt đất
           

            // Kiểm tra vận tốc và input để chuyển state
            float vy = rb.linearVelocity.y;
            if (currentState is IdleState)
            {
                if (IsRunningInput) 
                    ChangeState("Run");
                else if (vy > 0.01f)
                    ChangeState("Jump", true);
            }
            else if (currentState is RunState)
            {
                if (!IsRunningInput)
                    ChangeState("Idle");
                else if (vy > 0.01f)
                    ChangeState("Jump", true);
            }
            else if (currentState is JumpState && !IsGroundedLocal && vy < 0)
            {
                ChangeState("Jump");
            }
        }

        // Luôn gọi Update của state hiện tại để xử lý animation
        currentState?.Update(Time.deltaTime);
    
}
public void UpdateInput(float moveInput)
    {
        IsRunningInput = Mathf.Abs(moveInput) > 0.01f;
    }

}