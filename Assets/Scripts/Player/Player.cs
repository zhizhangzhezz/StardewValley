using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : SingletonMonobehaviour<Player>
{
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds afterPickAnimationPause;
    private AnimationOverrides animationOverrides;
    private GridCursor gridCursor;
    private Cursor cursor;
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

    private bool playerToolUseDisabled = false;
    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds pickAnimationPause;

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
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.tool, PartVariantType.hoe, PartVariantColor.none);

        characterAttributesCustomisationList = new List<CharacterAttribute>();

        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        //场景切换时禁用玩家移动
        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
    }
    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
    }

    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();

        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        pickAnimationPause = new WaitForSeconds(Settings.pickAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
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
            PlayerClickInput();
        }


    }

    // 移动放在 FixedUpdate
    private void FixedUpdate()
    {
        if (!PlayerInputIsDisabled)
        {
            PlayerMovement();

        }
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

    private void PlayerClickInput()
    {
        if (!playerToolUseDisabled)//播放使用工具动画时不能进行其他操作
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 处理左键点击
                if (gridCursor.CursorIsEnabled || cursor.CursorIsEnabled)
                {
                    //获取光标和玩家的网格位置
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();
                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }
        }
    }



    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetMovement();
        //获取光标相对玩家位置，确定动画播放的方向
        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);
        //获取网格属性
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        ItemDetails selectedItemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (selectedItemDetails != null)
        {
            switch (selectedItemDetails.itemType)
            {
                case ItemType.Seed:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSeed(gridPropertyDetails, selectedItemDetails);
                    }
                    break;

                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(selectedItemDetails);
                    }
                    break;

                case ItemType.HoeingTool:
                case ItemType.WateringTool:
                case ItemType.ReapingTool:
                case ItemType.CollectingTool:
                    ProcessPlayerClickInputTool(gridPropertyDetails, selectedItemDetails, playerDirection);
                    break;

                case ItemType.none:
                    break;

                case ItemType.count:
                    break;

                default:
                    break;
            }
        }
    }

    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        if (cursorGridPosition.x > playerGridPosition.x)
        {
            return Vector3Int.right;
        }
        else if (cursorGridPosition.x < playerGridPosition.x)
        {
            return Vector3Int.left;
        }
        else if (cursorGridPosition.y > playerGridPosition.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
    }

    private Vector3Int GetPlayerDirection(Vector3 cursorWorldPosition, Vector3 playerWorldPosition)
    {
        if (cursorWorldPosition.x > playerWorldPosition.x)
        {
            return Vector3Int.right;
        }
        else if (cursorWorldPosition.x < playerWorldPosition.x)
        {
            return Vector3Int.left;
        }
        else if (cursorWorldPosition.y > playerWorldPosition.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
        // if (cursorWorldPosition.x > playerWorldPosition.x &&
        // cursorWorldPosition.y < (playerWorldPosition.y + cursor.ItemUseRadius / 2f) &&
        // cursorWorldPosition.y > (playerWorldPosition.y - cursor.ItemUseRadius / 2f))
        // {
        //     return Vector3Int.right;
        // }
        // else if (cursorWorldPosition.x < playerWorldPosition.x &&
        // cursorWorldPosition.y < (playerWorldPosition.y + cursor.ItemUseRadius / 2f) &&
        // cursorWorldPosition.y > (playerWorldPosition.y - cursor.ItemUseRadius / 2f))
        // {
        //     return Vector3Int.left;
        // }
        // else if (cursorWorldPosition.y > playerWorldPosition.y)
        // {
        //     return Vector3Int.up;
        // }
        // else
        // {
        //     return Vector3Int.down;
        // }
    }

    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.seedItemCode == -1)
        {
            PlantSeedAtCursor(gridPropertyDetails, itemDetails);
        }
        else if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }

    //在光标处种下种子
    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        gridPropertyDetails.seedItemCode = itemDetails.itemCode;
        gridPropertyDetails.growthDays = 0;
        GridPropertiesManager.Instance.DisplayPlantedCrops(gridPropertyDetails);

        EventHandler.CallRemoveSelectedItemFromInventoryEvent();
    }

    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }
    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        switch (itemDetails.itemType)
        {
            case ItemType.HoeingTool:
                if (gridCursor.CursorPositionIsValid)
                {
                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.WateringTool:
                if (gridCursor.CursorPositionIsValid)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;
            case ItemType.CollectingTool:
                if (gridCursor.CursorPositionIsValid)
                {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.ReapingTool:
                if (cursor.CursorPositionIsValid)
                {
                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(), GetPlayerCenterPosition());
                    ReapInPlayerDirection(itemDetails, playerDirection);
                }
                break;
            default:
                break;
        }
    }

    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        //播放动画
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        playerToolUseDisabled = true;
        PlayerInputIsDisabled = true;
        //覆盖动画
        toolCharacterAttribute.partVariantType = PartVariantType.hoe;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        if (playerDirection == Vector3Int.up)
        {
            parameters.isUsingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            parameters.isUsingToolDown = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            parameters.isUsingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.right)
        {
            parameters.isUsingToolRight = true;
        }

        yield return useToolAnimationPause;
        //网格属性设为dug
        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        yield return afterUseToolAnimationPause;

        playerToolUseDisabled = false;
        PlayerInputIsDisabled = false;
    }

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        //播放动画
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        playerToolUseDisabled = true;
        PlayerInputIsDisabled = true;
        //覆盖动画
        toolCharacterAttribute.partVariantType = PartVariantType.wateringCan;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        parameters.toolEffect = ToolEffect.watering;

        if (playerDirection == Vector3Int.up)
        {
            parameters.isLiftingToolUp = true;
        }
        else if (playerDirection == Vector3Int.down)
        {
            parameters.isLiftingToolDown = true;
        }
        else if (playerDirection == Vector3Int.left)
        {
            parameters.isLiftingToolLeft = true;
        }
        else if (playerDirection == Vector3Int.right)
        {
            parameters.isLiftingToolRight = true;
        }

        yield return liftToolAnimationPause;
        //网格属性设为watered
        if (gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered = 0;
        }
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);

        yield return afterLiftToolAnimationPause;

        playerToolUseDisabled = false;
        PlayerInputIsDisabled = false;
    }

    private void ReapInPlayerDirection(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        //播放动画
        StartCoroutine(ReapInPlayerDirectionRoutine(itemDetails, playerDirection));
    }
    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        //播放动画
        StartCoroutine(CollectInPlayerDirectionRoutine(gridPropertyDetails, itemDetails, playerDirection));
    }

    private IEnumerator ReapInPlayerDirectionRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        playerToolUseDisabled = true;
        PlayerInputIsDisabled = true;

        //覆盖动画
        toolCharacterAttribute.partVariantType = PartVariantType.scythe;
        characterAttributesCustomisationList.Clear();
        characterAttributesCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributesCustomisationList);

        UseToolInPlayerDirection(itemDetails, playerDirection);

        yield return useToolAnimationPause;

        playerToolUseDisabled = false;
        PlayerInputIsDisabled = false;
    }

    private IEnumerator CollectInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        playerToolUseDisabled = true;
        PlayerInputIsDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);

        yield return pickAnimationPause;

        yield return afterPickAnimationPause;

        playerToolUseDisabled = false;
        PlayerInputIsDisabled = false;
    }

    private void ProcessCropWithEquippedItemInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        switch (itemDetails.itemType)
        {
            case ItemType.CollectingTool:
                if (playerDirection == Vector3Int.right)
                {
                    parameters.isPickingRight = true;
                }
                else if (playerDirection == Vector3Int.left)
                {
                    parameters.isPickingLeft = true;
                }
                else if (playerDirection == Vector3Int.up)
                {
                    parameters.isPickingUp = true;
                }
                else if (playerDirection == Vector3Int.down)
                {
                    parameters.isPickingDown = true;
                }
                break;
            case ItemType.none:
                break;
        }

        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        if (crop != null)
        {
            switch (itemDetails.itemType)
            {
                case ItemType.CollectingTool:
                    crop.ProcessToolAction(itemDetails);
                    break;
            }
        }
    }

    private void UseToolInPlayerDirection(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        if (Input.GetMouseButton(0))
        {
            switch (itemDetails.itemType)
            {
                case ItemType.ReapingTool:
                    if (playerDirection == Vector3Int.left)
                    {
                        parameters.isSwingingToolLeft = true;
                    }
                    else if (playerDirection == Vector3Int.right)
                    {
                        parameters.isSwingingToolRight = true;
                    }
                    else if (playerDirection == Vector3Int.up)
                    {
                        parameters.isSwingingToolUp = true;
                    }
                    else if (playerDirection == Vector3Int.down)
                    {
                        parameters.isSwingingToolDown = true;
                    }
                    break;
            }

            Vector2 point = new Vector2(GetPlayerCenterPosition().x + (playerDirection.x * (itemDetails.itemUseRadius / 2f)), GetPlayerCenterPosition().y + (playerDirection.y * (itemDetails.itemUseRadius / 2f)));

            Vector2 size = new Vector2(itemDetails.itemUseRadius, itemDetails.itemUseRadius);//挥舞范围

            Item[] itemArray = HelperMethods.GetComponentsAtBoxLocationNonAlloc<Item>(Settings.maxCollidersToTestPerSwing, point, size, 0f);

            int reapableItemCount = 0;

            for (int i = 0; i < itemArray.Length; i++)
            {
                if (itemArray[i] != null && InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.Reapable_scenary)
                {
                    //粒子效果范围
                    Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x, itemArray[i].transform.position.y + Settings.gridCellSize / 2f, itemArray[i].transform.position.z);
                    //触发粒子效果
                    EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.reaping);

                    Destroy(itemArray[i].gameObject);
                    reapableItemCount++;

                    if (reapableItemCount >= Settings.maxTargetToDestroyPerSwing)
                    {
                        break;
                    }
                }
            }
        }
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

    public Vector3 GetPlayerCenterPosition()
    {
        return new Vector3(transform.position.x, transform.position.y + Settings.playerCenterOffset, transform.position.z);
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
