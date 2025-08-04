using Assets.Script;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class RunState : BasePlayerAnimationState
{
    public RunState(BasePlayerController controller) : base(controller) { }

    public override string GetStateName() => "Run";

    public override void Enter()
    {
        base.Enter(); // Đặt frame = 0 và gọi SetFrame(0)
    }

    public override void Update(float deltaTime)
    {
       
            // Local player: Animation dựa trên frameRate, chuyển state đã được xử lý trong PlayerController
            base.Update(deltaTime); // Dùng logic animation tuần tự từ base
        
        //    // Remote player: Chỉ chạy animation tuần tự, không cần kiểm tra vật lý
        //    timer += deltaTime;
        //    if (timer >= frameRate)
        //    {
        //        timer -= frameRate;
        //        currentFrame = (currentFrame + 1) % controller.GetMaxFrameCount(GetStateName());
        //        controller.SetFrame(currentFrame);
        //    }
        //    //base.Update(deltaTime);
        //}
    }
}