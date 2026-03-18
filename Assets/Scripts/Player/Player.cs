using System.Collections.Generic;
using UnityEngine;

public class Player : SingletonMonobehaviour<Player>
{
    private AnimationOverrides animationOverrides;
    private MovementEventParams parameters = new MovementEventParams();
    private Rigidbody2D rb;
    private Direction playerDirection;
    private float movementSpeed;
    private bool _playerInputIsDisbled = false;

    private List<CharacterAttribute> characterAttributesCustomisationList;
    [Tooltip("把装备对应的图片拖到预制体的SpriteRenderer中!")]
    [SerializeField] private SpriteRenderer equippedItemSpriteRenderer = null;

    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;

    private Camera mainCamera;

    private const float PIXELS_PER_UNIT = 16f;
    private const float PIXEL_STEP = 1f / PIXELS_PER_UNIT;

    public bool PlayerInputIsDisabled { get => _playerInputIsDisbled; set => _playerInputIsDisbled = value; }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        animationOverrides = GetComponentInChildren<AnimationOverrides>();

        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.arms, PartVariantType.none, PartVariantColor.none);
        characterAttributesCustomisationList = new List<CharacterAttribute>();

        mainCamera = Camera.main;
    }

    private void Update()
    {
        //if (PlayerInputIsDisabled) return;

        if (!PlayerInputIsDisabled)
        {
            PlayerMovementInput();
            ResetAnimationTriggers();
            PlayerWalkInput();

            PlayerTestInput();
        }


    }

    // 移动放在 FixedUpdate
    private void FixedUpdate()
    {

        PlayerMovement();
        EventHandler.CallMovementEvent(parameters);
    }


    private void PlayerMovement()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(horizontal, vertical).normalized;
        Vector2 move = inputDir * movementSpeed * Time.smoothDeltaTime;

        // 玩家平滑移动（不进行像素对齐，让相机处理）
        rb.MovePosition(rb.position + move);
    }
    private void LateUpdate()
    {

    }
    private void PlayerWalkInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            parameters.isWalking = true;
            parameters.isRunning = false;
            movementSpeed = Settings.walkingSpeed;
        }
        else
        {
            parameters.isWalking = false;
            parameters.isRunning = true;
            movementSpeed = Settings.runningSpeed;
        }
    }

    private void PlayerMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");



        // 干净的输入
        parameters.inputX = horizontal;
        parameters.inputY = vertical;

        // 状态
        if (horizontal != 0 || vertical != 0)
        {
            parameters.isIdle = false;

            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
                playerDirection = horizontal > 0 ? Direction.right : Direction.left;
            else
                playerDirection = vertical > 0 ? Direction.up : Direction.down;
        }
        else
        {
            parameters.isIdle = true;
            parameters.isWalking = false;
            parameters.isRunning = false;
        }
    }

    private void PlayerTestInput()
    {
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }
        if (Input.GetKey(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }
        if (Input.GetKey(KeyCode.L))
        {
            SceneControllerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(), transform.position);
        }
    }

    private void ResetAnimationTriggers()
    {
        parameters.isPickingDown = false;
        parameters.isPickingLeft = false;
        parameters.isPickingRight = false;
        parameters.isPickingUp = false;
        parameters.isUsingToolDown = false;
        parameters.isUsingToolLeft = false;
        parameters.isUsingToolRight = false;
        parameters.isUsingToolUp = false;
        parameters.isLiftingToolDown = false;
        parameters.isLiftingToolLeft = false;
        parameters.isLiftingToolRight = false;
        parameters.isLiftingToolUp = false;
        parameters.isSwingingToolDown = false;
        parameters.isSwingingToolLeft = false;
        parameters.isSwingingToolRight = false;
        parameters.isSwingingToolUp = false;
        parameters.toolEffect = ToolEffect.none;
    }

    private void ResetMovement()
    {
        parameters.inputX = 0f;
        parameters.inputY = 0f;
        parameters.isIdle = true;
        parameters.isWalking = false;
        parameters.isRunning = false;
    }

    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();

        EventHandler.CallMovementEvent(parameters);
    }

    public void DisablePlayerInput()
    {
        PlayerInputIsDisabled = true;
    }

    public void EnablePlayerInput()
    {
        PlayerInputIsDisabled = false;
    }

    public Vector3 GetPlayerViewportPosition()
    {
        return mainCamera.WorldToViewportPoint(transform.position);
    }

    public void ShowCarriedItem(int itemCode)
    {

        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        if (itemDetails != null)
        {
            equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
            equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

            armsCharacterAttribute.partVariantType = PartVariantType.carry;
            characterAttributesCustomisationList.Clear();
            characterAttributesCustomisationList.Add(armsCharacterAttribute);
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);
            parameters.isCarrying = true;

            EventHandler.CallMovementEvent(parameters);
        }
    }

    public void ClearCarriedItem()
    {
        equippedItemSpriteRenderer.sprite = null;
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);

        armsCharacterAttribute.partVariantType = PartVariantType.none;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(armsCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);
        parameters.isCarrying = false;

        EventHandler.CallMovementEvent(parameters);
    }
}