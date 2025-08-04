using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Assets.Script;
using System.Linq;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    private BasePlayerController baseController;
    private Rigidbody2D rb;
    private IdleMotion motion;
    private CharacterData characterData;
    private SkillManager skillManager;

    public float moveSpeed = 8f;
    public float jumpForce = 10f;
    public bool IsGrounded { get; private set; }

    private float moveInput;
    public Transform groundCheck;
    public LayerMask groundLayer;
    private float groundCheckRadius = 0.1f;

    private float lastSentDirection = 0f;
    public bool wasMoving = false;
  
    private bool facingRight = true;
    private TargetingSystem targetingSystem;
    private bool isRunningSoundPlaying = false;

    private void Awake()
    {
        baseController = GetComponent<BasePlayerController>();
        rb = GetComponent<Rigidbody2D>();
        motion = GetComponent<IdleMotion>();
        groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null)
            Debug.LogError("Missing GroundCheck object under Player prefab!");
        groundLayer = LayerMask.GetMask("Ground");
    }

    private void Start()
    {
        characterData = GetComponent<CharacterData>();
        skillManager = SkillManager.Instance;
        targetingSystem = GetComponent<TargetingSystem>();
    }

    private void Update()
    {
        if (!(baseController is PlayerController)) return;
        if (baseController is PlayerController playerController)
        {
            playerController.UpdateInput(moveInput);
        }
        moveInput = Input.GetAxisRaw("Horizontal");
        bool isMoving = Mathf.Abs(moveInput) > 0.01f;
        float currentDir = Mathf.Sign(moveInput);
        Vector2 currentPosition = transform.position;

        // Xử lý lật hướng sprite
        if (isMoving && (facingRight != (moveInput > 0)))
        {
            facingRight = moveInput > 0;
            transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
        }

        HandleRunningSound(isMoving);
        // Gửi khi bắt đầu di chuyển hoặc đổi hướng
        if (isMoving)
        {
            if (!wasMoving || currentDir != lastSentDirection)
            {
                SendMoveState(true, currentDir, currentPosition);
                wasMoving = true;
                lastSentDirection = currentDir;
            }
            motion.enabled = false;
        }
        else
        {
            if (wasMoving)
            {
                SendMoveState(false, lastSentDirection, currentPosition);
                wasMoving = false;
            }
            motion.enabled = true;
        }

        // Jump
        Jump();

        // Attack
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            EnterTarget();
        }

    }

    private void FixedUpdate()
    {
        // Di chuyển nhân vật theo input
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void SendMoveState(bool isMoving, float direction, Vector2 position)
    {
        if (characterData == null) return;

        characterData.X = position.x;
        characterData.Y = position.y;

        var data = new JObject
        {
            ["movespeed"] = moveSpeed,
            ["isMoving"] = isMoving,
            ["dir"] = direction,
            ["x"] = position.x,
            ["y"] = position.y,
        };

        var message = new BaseMessage
        {
            Action = "Charactermove",
            Data = data
        };

        string json = JsonConvert.SerializeObject(message);
        ClientManager.Instance.Send(json);
    }

    private void Jump()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
           
            motion.enabled = false;

            // Gửi trạng thái nhảy lên server (nếu cần)
            SendJumpState();
        }
    }

    private void SendJumpState()
    {
        var data = new JObject
        {
            ["force"] = jumpForce,           
           
        };
        var message = new BaseMessage
        {
            Action = "CharacterJump",
            Data = data
        };
        string json = JsonConvert.SerializeObject(message);
        ClientManager.Instance.Send(json);
        Debug.Log(json);
    }
    public void EnterTarget()
    {
        var target = targetingSystem.GetCurrentTarget();
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (target == null)
        {
            Debug.Log("Target null");
            return;
        }
        switch (target.tag)
        {

            case "Monster":
                {
                    Attack(target);
                    break;
                }
            case "NPC":
                {
                  

                    if (distance > 2f)
                    {
                        ToastManager.Instance.ShowToast("Bạn đang ở quá xa", "error");
                        return;
                    }
                    var id = target.GetComponent<Health>().id;
                    UIManager.Instance.OpenNPCDialog(id);
                    Debug.Log("Target is NPC");
                    break;
                }
            case "Item":
                {

                    if (distance > 2f)
                    {
                        ToastManager.Instance.ShowToast("Bạn đang ở quá xa Vật phẩm", "error");
                        return;
                    }
                    SendPickup(target);
                    break;
                }
            case "Player":
                {
                    TradeManager.instance.ShowTradePanel();
                    break;
                }
        }

    }
    private void HandleRunningSound(bool isCurrentlyMoving)
    {
        // Chỉ xử lý âm thanh nếu AudioManager đã tồn tại
        if (AudioManager.Instance == null) return;

        // Điều kiện để phát âm thanh: đang di chuyển VÀ đang trên mặt đất
        bool shouldPlaySound = isCurrentlyMoving && IsGrounded;

        if (shouldPlaySound)
        {
            // Nếu nên phát nhưng âm thanh chưa chạy -> Bắt đầu phát
            if (!isRunningSoundPlaying)
            {
                AudioManager.Instance.PlaySFX("Run"); // Tên phải khớp với Inspector
                isRunningSoundPlaying = true;
            }
        }
        else
        {
            // Nếu không nên phát nhưng âm thanh vẫn đang chạy -> Dừng lại
            if (isRunningSoundPlaying)
            {
                AudioManager.Instance.StopSFX("Run");
                isRunningSoundPlaying = false;
            }
        }
    }

    private void Attack(GameObject target)
    {

        AudioManager.Instance.PlaySFX("Attack");
        bool skillFacing = (transform.position.x < target.transform.position.x);
        transform.localScale = new Vector3(skillFacing ? 1 : -1, 1, 1);
        if (characterData != null && characterData.Class == 0 || characterData.WeaponID <=0)
        {
            PopupManager.Instance.ShowPopup("Chưa mang vũ khí");
            return;
        }

        baseController.ChangeState("Attack");
        motion.enabled = false;
        var skillid = characterData.Class == 1 ? 101 : 102;
      
        skillManager.PlaySkill(skillid, transform.localPosition, skillFacing,target.transform,true);
        Debug.Log(target.transform.position);
        SendAttack(target.transform, skillid,skillFacing);
        // Gửi trạng thái tấn công nếu cần
      
    }
    public void SendAttack(Transform selectedTarget,int skillID,bool facingRight)
    {
        var monster = selectedTarget.gameObject.GetComponent<Health>();
        var targetID = monster.id;
        var message = new BaseMessage
        {
            Action = "Player_attack",
            Data = new Newtonsoft.Json.Linq.JObject
            {
                ["targetID"] = targetID,
                ["skillID"] = skillID,
                ["facingRight"] = facingRight,
            }
        };

        string json = JsonConvert.SerializeObject(message);
        Debug.Log(json);
        ClientManager.Instance.Send(json);
    }
    public void SendPickup(GameObject target)
    {
        // Sử dụng thuộc tính UniqueId (string) mà chúng ta đã định nghĩa
        var uniqueId = target.GetComponent<DroppedItem>()?.GroundItemID;

        if (string.IsNullOrEmpty(uniqueId))
        {
            Debug.LogError("Could not get UniqueId from the dropped item.", target);
            return;
        }

        var message = new BaseMessage
        {
            Action = "pickup_item", // Đảm bảo action này khớp với server
            Data = new JObject
            {
                ["uniqueId"] = uniqueId,
            }
        };

        // --- THÊM 2 DÒNG BỊ THIẾU VÀO ĐÂY ---
        string json = JsonConvert.SerializeObject(message);
        Debug.Log($"Sending pickup request: {json}"); // Thêm Log để dễ debug
        ClientManager.Instance.Send(json);
    }
}




