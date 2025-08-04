using Assets.Script;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class IdleState : BasePlayerAnimationState
{
    public IdleState(BasePlayerController controller) : base(controller) { }

    public override string GetStateName() => "Idle";

    public override void Enter()
    {
        currentFrame = 0;
        controller.SetFrame(currentFrame);
    }

    public override void Update(float deltaTime)
    {
        if (controller is PlayerController playerController)
        {
            float vy = playerController.rb.linearVelocity.y;
           if(vy < 0 && !playerController.IsGroundedLocal)
            {
                controller.ChangeState("Jump",true);
            }
            if (playerController.IsRunningInput)
            {
                controller.ChangeState("Run", true);
            }
        }
        else
        {

        }
    }
}