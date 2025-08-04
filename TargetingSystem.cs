using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Assets.Script;

public class TargetingSystem : MonoBehaviour
{
    public float tabSelectRadius = 10f;   // Bán kính tìm mục tiêu xung quanh player khi bấm Tab
    public LayerMask targetLayerMask;  // Layer của các target (hoặc dùng tag filter trong code)

    private List<GameObject> targets = new List<GameObject>();
    private int currentTargetIndex = -1;

    private float targetRefreshInterval = 1f;  // 1 giây làm mới danh sách target
    private float lastTargetRefreshTime = 0f;
    private TargetInfor targetInfor;


    void Awake()
    {
        targetLayerMask = LayerMask.GetMask("Monster", "NPC", "Item","Player");
        
    }
    public void SetTargetInfor(TargetInfor infor)
    {
        targetInfor = infor;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectTargetByMouse();
        }

        // Cập nhật danh sách target theo thời gian, không phải mỗi lần bấm Tab
        if (Time.time - lastTargetRefreshTime > targetRefreshInterval)
        {
            RefreshTargets();
            lastTargetRefreshTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleTargets();
        }

        // Nếu target hiện tại đi quá xa, bỏ target
        if (currentTarget != null )
        {
            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (dist > tabSelectRadius || !currentTarget.activeInHierarchy)
            {
                ClearTarget();
                CycleTargets();
            }
        }
    }

    void TrySelectTargetByMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos, targetLayerMask);
        if (col != null && IsValidTarget(col.gameObject))
        {
            SetTarget(col.gameObject);
        }
    }


    void RefreshTargets()
    {
        targets.Clear();
        currentTargetIndex = -1;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, tabSelectRadius, targetLayerMask);
        foreach (var col in colliders)
        {
            if (IsValidTarget(col.gameObject))
            {
                targets.Add(col.gameObject);
            }
        }

        targets.Sort((a, b) =>
        {
            float distA = Vector2.Distance(transform.position, a.transform.position);
            float distB = Vector2.Distance(transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });
    }

    void CycleTargets()
    {
        if (targets.Count == 0)
        {
            ClearTarget();
            return;
        }

        currentTargetIndex++;
        if (currentTargetIndex >= targets.Count)
            currentTargetIndex = 0;

        SetTarget(targets[currentTargetIndex]);
    }

    //void FindTargetsAround()
    //{
    //    targets.Clear();
    //    currentTargetIndex = 0;

    //    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, tabSelectRadius, targetLayerMask);
    //    foreach (var col in colliders)
    //    {
    //        if (IsValidTarget(col.gameObject))
    //        {
    //            targets.Add(col.gameObject);
    //        }
    //    }

    //    // Sắp xếp theo khoảng cách gần nhất nếu muốn
    //    targets.Sort((a, b) =>
    //    {
    //        float distA = Vector2.Distance(transform.position, a.transform.position);
    //        float distB = Vector2.Distance(transform.position, b.transform.position);
    //        return distA.CompareTo(distB);
    //    });
    //}

    bool IsValidTarget(GameObject obj)
    {
        if (obj == null) return false;
        if (!obj.activeInHierarchy) return false;

        string tag = obj.tag;
        return tag == "Monster" || tag == "NPC" || tag == "Item" || tag == "Player";
    }

    private GameObject currentTarget;

    public void SetTarget(GameObject newTarget)
    {
       // if (currentTarget == newTarget  ) return;

        // Bỏ target cũ
        if (currentTarget != null)
        {
            var target = currentTarget.GetComponent<Health>();
            target?.SetTargeted(false);
        }

        currentTarget = newTarget;

        // Gán target mới
        var newtarget = currentTarget.GetComponent<Health>();
        targetInfor.SetTarget(newtarget);
        newtarget?.SetTargeted(true);

        switch (newtarget.tag)
        {
            case "Player": 
            {
                    Debug.Log($"target player {newtarget.id}");
                    break;
            }
            case "Monster":
                {
                    Debug.Log($"target Monster {newtarget.id}");
                    break;
                }
            case "NPC":
                {
                    Debug.Log($"target NPC {newtarget.id}");
                    break;
                }
        }
 
    }


    public void ClearTarget()
    {
        if (currentTarget != null)
        {
            var monster = currentTarget.GetComponent<Health>();
            targetInfor.Clear();
            monster?.SetTargeted(false);
        }

        currentTarget = null;
    }
    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }


}
