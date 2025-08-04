using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;
using Assets.Script;
using Newtonsoft.Json;

public abstract  class BasePlayerController : MonoBehaviour
{
    protected BasePlayerAnimationState currentState;
    protected Dictionary<string, BasePlayerAnimationState> states = new Dictionary<string, BasePlayerAnimationState>();
    public SpriteRenderer headRenderer, bodyRenderer, legRenderer, weaponRenderer, spinRenderer, deadRenderer,myPlayerRenderer;
    protected Dictionary<string, List<Sprite>> headSprites, bodySprites, legSprites, weaponSprites, spinSprites;
   
    protected int currentFrame;
    public Rigidbody2D rb;
   
    public bool IsStateLocked { get; protected set; }


 
   

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
      
        states["Idle"] = new IdleState(this);
        states["Run"] = new RunState(this);
        states["Attack"] = new AttackState(this);
        states["Jump"] = new JumpState(this);
        states["Hit"] = new HitState(this);

        ChangeState("Idle");


    }
    void Update()
    {
        if (currentState != null)
            currentState.Update(Time.deltaTime);
    }

    public string GetCurrentStateName()
    {
        return currentState?.GetStateName();
    }
  
    public void Init()
    {
        headRenderer = transform.Find("Head")?.GetComponent<SpriteRenderer>();
        bodyRenderer = transform.Find("Body")?.GetComponent<SpriteRenderer>();
        legRenderer = transform.Find("Leg")?.GetComponent<SpriteRenderer>();
        weaponRenderer = transform.Find("Weapon")?.GetComponent<SpriteRenderer>();
        spinRenderer = transform.Find("SpinJump")?.GetComponent<SpriteRenderer>();
        deadRenderer = transform.Find("Dead")?.GetComponent<SpriteRenderer>();
        myPlayerRenderer = transform.Find("Myplayer")?.GetComponent<SpriteRenderer>();
        deadRenderer.enabled = false;
        myPlayerRenderer.enabled = false;
    }

    public void SetSprites(Dictionary<string, List<Sprite>> head, Dictionary<string, List<Sprite>> body,
                          Dictionary<string, List<Sprite>> leg, Dictionary<string, List<Sprite>> weapon)
    {
        headSprites = head;
        bodySprites = body;
        legSprites = leg;
        weaponSprites = weapon ?? new Dictionary<string, List<Sprite>>();
        SetFrame(0);
    }

    public void SetSpinSprites(Dictionary<string, List<Sprite>> sprites)
    {
        spinSprites = sprites;
    }
     

    public void ChangeState(string state, bool force = false)
    {
        if (currentState?.GetStateName() == state || (IsStateLocked && state != "Idle" && !force))
            return;

        currentState?.Exit();
        if (states.TryGetValue(state, out var nextState))
        {
            currentState = nextState;
            currentState.Enter();
            SetFrame(0);
        }
        else
        {
         
        }
    }

    public void SetFrame(int frame)
    {
        currentFrame = frame;
        if (headSprites == null || bodySprites == null || legSprites == null)
            return;

        SetSprite(headRenderer, headSprites, frame);
        SetSprite(bodyRenderer, bodySprites, frame);
        SetSprite(legRenderer, legSprites, frame);

        if (currentState?.GetStateName() != "Attack")
        {
            if (weaponRenderer != null)
            {
                weaponRenderer.enabled = true;
                SetSprite(weaponRenderer, weaponSprites, frame);
            }
        }
        else
        {
            if (weaponRenderer != null)
                weaponRenderer.enabled = false;
        }
    }
    public void SetSpinFrame(int frame)
    {
        currentFrame = frame;
        SetSpinSprite( spinSprites, frame);
    }
    private void SetSprite(SpriteRenderer renderer, Dictionary<string, List<Sprite>> sprites, int frame)
    {
        if (currentState == null || !sprites.ContainsKey(currentState.GetStateName()))
            return;

        List<Sprite> frames = sprites[currentState.GetStateName()];
        if (frames == null || frames.Count == 0)
            return;

        int clampedFrame = Mathf.Clamp(frame, 0, frames.Count-1);
        renderer.sprite = frames[clampedFrame];
    }

    public void SetSpinSprite(Dictionary<string, List<Sprite>> sprites, int frame)
    {
        if (spinRenderer == null || spinSprites == null || spinSprites.Count == 0)
        {
            spinRenderer.enabled = false;
            return;
        }
        if (!sprites.TryGetValue(currentState.GetStateName(), out var frames))
        {
            spinRenderer.enabled = false;
            return;
        }

        int clampedFrame = Mathf.Clamp(frame, 0, frames.Count - 1);
        spinRenderer.enabled = true;
        spinRenderer.sprite = frames[clampedFrame];
    }

    public void HideSpin()
    {
        if (spinRenderer != null)
            spinRenderer.enabled = false;
    }

    public void ShowOnlySpin()
    {
        HideModular();
        spinRenderer.enabled = true;
    }

    public void ShowDeadOnly()
    {
        HideModular();
        deadRenderer.enabled = true;
        weaponRenderer.enabled = false;
    }
    public void ShowMyPlayer()
    {
        myPlayerRenderer.enabled = true;
    }
    public void HideMyPlayer()
    {
        myPlayerRenderer.enabled = false;
    }
    public void HideDead()
    {
        deadRenderer.enabled = false;
        weaponRenderer.enabled = true;
        ShowModular();
    }

    public void ShowModular()
    {
        headRenderer.enabled = true;
        bodyRenderer.enabled = true;
        legRenderer.enabled = true;
        if (weaponRenderer != null)
            weaponRenderer.enabled = true;
    }

    public void HideModular()
    {
        headRenderer.enabled = false;
        bodyRenderer.enabled = false;
        legRenderer.enabled = false;
        if (weaponRenderer != null)
        weaponRenderer.enabled = false;
    }

    public void LockState(float duration)
    {
        StartCoroutine(UnlockAfterDelay(duration));
    }

    private IEnumerator UnlockAfterDelay(float time)
    {
        IsStateLocked = true;
        yield return new WaitForSeconds(time);
        IsStateLocked = false;
        ChangeState("Idle");
    }

    public int GetMaxFrameCount(string state)
    {
        int max = 1;
        if (headSprites != null && headSprites.TryGetValue(state, out var headList) && headList.Count > 0)
            max = Mathf.Max(max, headList.Count);
        if (bodySprites != null && bodySprites.TryGetValue(state, out var bodyList) && bodyList.Count > 0)
            max = Mathf.Max(max, bodyList.Count);
        if (legSprites != null && legSprites.TryGetValue(state, out var legList) && legList.Count > 0)
            max = Mathf.Max(max, legList.Count);
       
        if (weaponSprites != null && weaponSprites.TryGetValue(state, out var weaponList) && weaponList.Count > 0)
            max = Mathf.Max(max, weaponList.Count);
        return max;
    }

    public int SpinSpritesCount()
    {
        if (spinSprites == null || currentState == null)
            return 0;
        if (spinSprites.TryGetValue(currentState.GetStateName(), out var frames))
        {
            return frames?.Count ?? 0;
        }
        return 0;
    }


  

    public void TriggerHit()
    {
        ChangeState("Hit", true);

        // Gửi sự kiện mạng nếu là local player
        if (this is PlayerController)
        {
            var characterData = GetComponent<CharacterData>();
            if (characterData != null)
            {
                var data = new BaseMessage
                {
                    Action = "CharacterHit",
                    Data = new JObject
                    {
                        ["characterId"] = characterData.CharacterID
                    }
                };
                string json = JsonConvert.SerializeObject(data);
                ClientManager.Instance.Send(json);
            }
        }
    }
}