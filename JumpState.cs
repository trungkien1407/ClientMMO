using Assets.Script;
using UnityEngine;

public class JumpState : BasePlayerAnimationState
{
    private float spinFrameRate = 0.05f;
    private float spinTimer;
    private int spinFrame;
    private bool isSpinning;
    private float prevVy;

    public JumpState(BasePlayerController controller) : base(controller)
    {
        frameRate = 0.45f; // Tốc độ animation jump
    }

    public override string GetStateName() => "Jump";

    public override void Enter()
    {
        base.Enter();
        spinTimer = 0f;
        spinFrame = 0;
        isSpinning = false;
        prevVy = 0f;

        controller.ShowModular();
        controller.HideSpin();

        float duration = (controller.GetMaxFrameCount("Jump")) * frameRate;
        controller.LockState(duration);

      
    }

    public override void Update(float deltaTime)
    {
        spinTimer += deltaTime;

        if (controller is PlayerController playerController)
        {
            float vy = playerController.rb.linearVelocity.y;

            // Bắt đầu spin nếu đang lên mà chuyển sang rơi
            if (!isSpinning && prevVy > 0f && vy < 0f)
            {
                isSpinning = true;
                spinFrame = 0;
                spinTimer = 0f;
                controller.HideModular();
                controller.ShowOnlySpin();

             
            }

            prevVy = vy;

            if (isSpinning)
            {
                if (spinTimer >= spinFrameRate)
                {
                    spinTimer -= spinFrameRate;
                    spinFrame++;
                    if (spinFrame < controller.SpinSpritesCount())
                    {
                        controller.SetSpinFrame(spinFrame);
                    }
                    else
                    {
                        isSpinning = false;
                        controller.HideSpin();
                        controller.ShowModular();
                    
                    }
                }
            }
            if (prevVy <= 0 && playerController.IsGroundedLocal == true)
            {


                controller.ChangeState("Idle", true);
            }
            else
            {
                // Cập nhật frame nhảy hoặc rơi ngay lập tức
                int newFrame = (vy < 0f) ? 1 : 0;
                if (currentFrame != newFrame)
                {
                    currentFrame = newFrame;
                    controller.SetFrame(currentFrame);
                  
                }
            }
        }
        else
            if (controller is RemoteController remote)
        {
            float vy = remote.rb.linearVelocity.y;

            // Bắt đầu spin nếu đang lên mà chuyển sang rơi
            if (!isSpinning && prevVy > 0f && vy < 0f)
            {
                isSpinning = true;
                spinFrame = 0;
                spinTimer = 0f;
                controller.HideModular();
                controller.ShowOnlySpin();

             
            }

            prevVy = vy;

            if (isSpinning)
            {
                if (spinTimer >= spinFrameRate)
                {
                    spinTimer -= spinFrameRate;
                    spinFrame++;
                    if (spinFrame < controller.SpinSpritesCount())
                    {
                        controller.SetSpinFrame(spinFrame);
                    }
                    else
                    {
                        isSpinning = false;
                        controller.HideSpin();
                        controller.ShowModular();
                       
                    }
                }
            }
            if (prevVy <= 0 && remote.isGrounded == true)
            {


                controller.ChangeState("Idle", true);
            }
            else
            {
                // Cập nhật frame nhảy hoặc rơi ngay lập tức
                int newFrame = (vy < 0f) ? 1 : 0;
                if (currentFrame != newFrame)
                {
                    currentFrame = newFrame;
                    controller.SetFrame(currentFrame);
                  
                }
            }
        }
    }


    public void StartSpin()
    {
        isSpinning = true;
        spinFrame = 0;
        spinTimer = 0f;
        controller.HideModular();
        controller.ShowOnlySpin();
    }
    public override void Exit()
    {
        base.Exit();
        isSpinning = false;
        controller.HideSpin();
        controller.ShowModular();
    }

}