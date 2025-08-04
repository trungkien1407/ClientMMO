using Assets.Script;
using UnityEngine;

public class AttackState : BasePlayerAnimationState
{
    public AttackState(BasePlayerController controller) : base(controller) { }

    public override string GetStateName() => "Attack";

    public override void Enter()
    {
        base.Enter();
       
        controller.SetFrame(currentFrame);
        int maxFrames = controller.GetMaxFrameCount("Attack");
        float duration = maxFrames * frameRate;
   
        controller.LockState(duration);
    }

    public override void Update(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrame++;
            int maxFrames = controller.GetMaxFrameCount("Attack");
            if (maxFrames <= 1)
            {
               
                return;
            }
            if (currentFrame >= maxFrames)
            {
                currentFrame = 0;
                controller.ChangeState("Idle", true);
               
            }
            else
            {
                controller.SetFrame(currentFrame);
              
            }
        }
    }
}