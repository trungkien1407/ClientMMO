using Assets.Script;
using UnityEngine;


    public abstract class BasePlayerAnimationState 
    {
        protected readonly BasePlayerController controller;
        protected float frameRate = 0.1f; // Tốc độ chuyển frame
        protected float timer;
        protected int currentFrame;

        protected BasePlayerAnimationState(BasePlayerController controller)
        {
            this.controller = controller;
        }

        public abstract string GetStateName();

        public virtual void Enter()
        {
            currentFrame = 0;
            timer = 0f;
            controller.SetFrame(currentFrame);
        }

        public virtual void Update(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= frameRate)
            {
                timer -= frameRate;
                currentFrame = (currentFrame + 1) % controller.GetMaxFrameCount(GetStateName());
                controller.SetFrame(currentFrame);
            }
        }

        public virtual void Exit() { }
    }
