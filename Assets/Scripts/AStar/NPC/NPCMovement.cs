using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NPCPath))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class NPCMovement : MonoBehaviour
{
    [HideInInspector] public SceneName npcCurrentScene;
    [HideInInspector] public SceneName npcTargetScene;
    [HideInInspector] public Vector3Int npcCurrentGridPosition;
    [HideInInspector] public Vector3Int npcTargetGridPosition;
    [HideInInspector] public Vector3 npcTargetWorldPosition;
    [HideInInspector] public Direction npcFacingDirectionAtDestination;

    private SceneName npcPreviousMovementStepScene;
    private Vector3Int npcNextGridPosition;
    private Vector3 npcNextWorldPosition;

    [Header("NPC Movement")]
    public float npcNormalSpeed = 2f;

    [SerializeField] private float npcMinSpeed = 1f;
    [SerializeField] private float npcMaxSpeed = 3f;
    private bool npcIsMoving = false;

    [HideInInspector] public AnimationClip npcTargetAnimationClip;

    [Header("NPC Animation")]
    [SerializeField] private AnimationClip blankAnimation = null;

    private Grid grid;
    private Rigidbody2D rigidbody2d;
    private BoxCollider2D boxCollider2D;
    private WaitForFixedUpdate waitForFixedUpdate;
    private Animator animator;
    private AnimatorOverrideController animatorOverrideController;
    private int lastMoveAnimationParameter;
    private NPCPath npcPath;
    private bool npcInitialized = false;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public bool npcActiveInScene = false;

    private bool sceneLoaded = false;

    private Coroutine moveToGridPositionCoroutine;

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnload;
    }

    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        npcPath = GetComponent<NPCPath>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        npcTargetScene = npcCurrentScene;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = transform.position;
    }

    private void AfterSceneLoad()
    {
        grid = GameObject.FindObjectOfType<Grid>();

        if (!npcInitialized)
        {
            InitializeNPC();
            npcInitialized = true;
        }
        sceneLoaded = true;
    }
    private void BeforeSceneUnload()
    {
        sceneLoaded = false;
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
        SetIdleAnimation();
    }

    private void SetIdleAnimation()
    {
        animator.SetBool(Settings.idleDown, true);
    }

    private void FixedUpdate()
    {
        if (sceneLoaded)
        {
            if (!npcIsMoving)
            {
                //设置npc当前格子坐标和下一个格子坐标
                npcCurrentGridPosition = GetGridPosition(transform.position);
                npcNextGridPosition = npcCurrentGridPosition;

                if (npcPath.nPCMovementSteps.Count > 0)
                {
                    NPCMovementStep npcMovementStep = npcPath.nPCMovementSteps.Peek();
                    npcCurrentScene = npcMovementStep.sceneName;

                    //Debug.Log($"【NPC Debug】路径栈剩余步数: {npcPath.nPCMovementSteps.Count} | 当前场景: {npcCurrentScene} | 目标点坐标: {npcMovementStep.gridCoordinate}");

                    //若npc即将移动到新场景,重置到新场景的起始坐标
                    if (npcCurrentScene != npcPreviousMovementStepScene)
                    {
                        //Debug.LogWarning($"【NPC Debug】!!! 场景切换触发瞬移 !!! 旧场景: {npcPreviousMovementStepScene} → 新场景: {npcCurrentScene} 瞬移坐标: {npcMovementStep.gridCoordinate}");

                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);
                        npcPreviousMovementStepScene = npcCurrentScene;
                        npcPath.UpdateTimesOnPath();
                    }

                    //移动npc到下一个格子坐标(与玩家同场景)
                    if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
                    {
                        //Debug.Log($"【NPC Debug】NPC 在当前可见场景 → 开始移动");

                        SetNPCActiveInScene();
                        npcMovementStep = npcPath.nPCMovementSteps.Pop();
                        npcNextGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;

                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);
                        MoveToGridPosition(npcNextGridPosition, npcMovementStepTime, TimeManager.Instance.GetGameTime());
                    }
                    else//(不同场景)
                    {
                        //Debug.Log($"【NPC Debug】NPC 在其他场景 → 隐藏并快速跳格");
                        SetNPCInactiveInScene();

                        npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                        npcNextGridPosition = npcCurrentGridPosition;
                        transform.position = GetWorldPosition(npcCurrentGridPosition);

                        TimeSpan npcMovementStepTime = new TimeSpan(npcMovementStep.hour, npcMovementStep.minute, npcMovementStep.second);
                        TimeSpan gameTime = TimeManager.Instance.GetGameTime();
                        // Debug.Log($"【NPC Debug】路径时间: {npcMovementStepTime} 当前时间: {gameTime}");

                        if (npcMovementStepTime < gameTime)
                        {
                            //Debug.LogWarning($"【NPC Debug】!!! 时间过期 → 直接跳步 !!!");

                            npcMovementStep = npcPath.nPCMovementSteps.Pop();

                            npcCurrentGridPosition = (Vector3Int)npcMovementStep.gridCoordinate;
                            npcNextGridPosition = npcCurrentGridPosition;
                            transform.position = GetWorldPosition(npcCurrentGridPosition);
                        }
                    }
                }
                else
                {
                    //Debug.Log($"【NPC Debug】路径已走完 → 停止移动 目标坐标: {npcTargetGridPosition}");

                    ResetMoveAnimation();
                    SetNPCFacingDirection();
                    SetNPCEventAnimation();
                }
            }
        }
    }

    public void SetNPCActiveInScene()
    {
        spriteRenderer.enabled = true;
        boxCollider2D.enabled = true;
        npcActiveInScene = true;
    }
    public void SetNPCInactiveInScene()
    {
        spriteRenderer.enabled = false;
        boxCollider2D.enabled = false;
        npcActiveInScene = false;
    }

    private void MoveToGridPosition(Vector3Int targetGridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
    {
        moveToGridPositionCoroutine = StartCoroutine(MoveToGridPositionCoroutine(targetGridPosition, npcMovementStepTime, gameTime));
    }

    private IEnumerator MoveToGridPositionCoroutine(Vector3Int targetGridPosition, TimeSpan npcMovementStepTime, TimeSpan gameTime)
    {
        npcIsMoving = true;

        SetMoveAnimation(targetGridPosition);

        npcNextWorldPosition = GetWorldPosition(targetGridPosition);

        if (npcMovementStepTime > gameTime)
        {
            //计算移动速度，移动npc
            float timeToMove = (float)(npcMovementStepTime.TotalSeconds - gameTime.TotalSeconds);
            float npcCalculateSpeed = Mathf.Max(npcMinSpeed, Vector3.Distance(transform.position, npcNextWorldPosition) / timeToMove / Settings.secondsPerGameScond);

            if (npcCalculateSpeed <= npcMaxSpeed)
            {
                while (Vector3.Distance(transform.position, npcNextWorldPosition) > Settings.pixelSize)
                {
                    Vector3 unitVector = Vector3.Normalize(npcNextWorldPosition - transform.position);
                    Vector2 move = new Vector2(unitVector.x * npcCalculateSpeed * Time.fixedDeltaTime, unitVector.y * npcCalculateSpeed * Time.fixedDeltaTime);

                    rigidbody2d.MovePosition(rigidbody2d.position + move);

                    yield return waitForFixedUpdate;
                }
            }
        }

        rigidbody2d.position = npcNextWorldPosition;
        npcCurrentGridPosition = targetGridPosition;
        npcNextGridPosition = npcCurrentGridPosition;
        npcIsMoving = false;
    }

    private void SetMoveAnimation(Vector3Int targetGridPosition)
    {
        ResetIdleAnimation();
        ResetMoveAnimation();
        //获取并设置方向
        Vector3 toWorldPosition = GetWorldPosition(targetGridPosition);
        Vector3 directionVector = toWorldPosition - transform.position;

        if (Mathf.Abs(directionVector.x) >= Mathf.Abs(directionVector.y))
        {
            if (directionVector.x > 0)
            {
                animator.SetBool(Settings.walkRight, true);
            }
            else
            {
                animator.SetBool(Settings.walkLeft, true);
            }
        }
        else
        {
            if (directionVector.y > 0)
            {
                animator.SetBool(Settings.walkUp, true);
            }
            else
            {
                animator.SetBool(Settings.walkDown, true);
            }
        }
    }

    private void ResetMoveAnimation()
    {
        animator.SetBool(Settings.walkUp, false);
        animator.SetBool(Settings.walkDown, false);
        animator.SetBool(Settings.walkLeft, false);
        animator.SetBool(Settings.walkRight, false);
    }
    private void ResetIdleAnimation()
    {
        animator.SetBool(Settings.idleUp, false);
        animator.SetBool(Settings.idleDown, false);
        animator.SetBool(Settings.idleLeft, false);
        animator.SetBool(Settings.idleRight, false);
    }
    private void SetNPCFacingDirection()
    {
        ResetIdleAnimation();

        switch (npcFacingDirectionAtDestination)
        {
            case Direction.up:
                animator.SetBool(Settings.idleUp, true);
                break;
            case Direction.down:
                animator.SetBool(Settings.idleDown, true);
                break;
            case Direction.left:
                animator.SetBool(Settings.idleLeft, true);
                break;
            case Direction.right:
                animator.SetBool(Settings.idleRight, true);
                break;
        }
    }
    private void SetNPCEventAnimation()
    {
        //设置npc动画
        if (npcTargetAnimationClip != null)
        {
            ResetIdleAnimation();
            animatorOverrideController[blankAnimation] = npcTargetAnimationClip;
            animator.SetBool(Settings.eventAnimation, true);
        }
        else
        {
            animatorOverrideController[blankAnimation] = blankAnimation;
            animator.SetBool(Settings.eventAnimation, false);
        }
    }

    private Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        if (grid != null)
        {
            return grid.WorldToCell(worldPosition);
        }
        return Vector3Int.zero;
    }
    private Vector3 GetWorldPosition(Vector3Int gridPosition)
    {
        Vector3 worldPosition = grid.CellToWorld(gridPosition);
        return new Vector3(worldPosition.x + Settings.gridCellSize / 2f, worldPosition.y + Settings.gridCellSize / 2f, worldPosition.z);
    }

    private void InitializeNPC()
    {
        if (npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
        {
            SetNPCActiveInScene();
        }
        else
        {
            SetNPCInactiveInScene();
        }
        //初始化npc格子和世界坐标
        npcCurrentGridPosition = GetGridPosition(transform.position);
        npcNextGridPosition = npcCurrentGridPosition;
        npcTargetGridPosition = npcCurrentGridPosition;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);
        npcNextWorldPosition = GetWorldPosition(npcCurrentGridPosition);
        npcPreviousMovementStepScene = npcCurrentScene;
    }

    public void SetScheduleEventDetails(NPCScheduleEvent scheduleEvent)
    {
        npcTargetScene = scheduleEvent.toSceneName;
        npcTargetGridPosition = (Vector3Int)scheduleEvent.toGridCoordinate;
        npcTargetWorldPosition = GetWorldPosition(npcTargetGridPosition);
        npcFacingDirectionAtDestination = scheduleEvent.npcFacingDirectionAtDestination;
        npcTargetAnimationClip = scheduleEvent.animationAtDestination;
        ClearNPCEventAnimation();
    }

    public void ClearNPCEventAnimation()
    {
        animatorOverrideController[blankAnimation] = blankAnimation;
        animator.SetBool(Settings.eventAnimation, false);

        transform.rotation = Quaternion.identity;
    }
}

