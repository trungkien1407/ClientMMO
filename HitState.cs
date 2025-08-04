using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script
{
    public class HitState : BasePlayerAnimationState
    {
        public HitState(BasePlayerController controller) : base(controller)
        {
            frameRate = float.MaxValue; // Chỉ có 1 frame
        }
        public override string GetStateName() => "Hit";

        public override void Enter()
        {
            base.Enter();
            controller.LockState(0.15f);
        }

        public override void Update(float deltaTime)
        {
            // Không cập nhật frame, giữ frame đầu tiên
        }
    }
}
