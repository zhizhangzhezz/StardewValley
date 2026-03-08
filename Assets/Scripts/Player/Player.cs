using UnityEngine;

public class Player : SingletonMonobehaviour<Player>
{
    private MovementEventParams parameters = new MovementEventParams();
    private Rigidbody2D rb;
    private Direction playerDirection;
    private float movementSpeed;
    private bool _playerInputIsDisbled = false;

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

        mainCamera = Camera.main;
    }

    private void Update()
    {
        //if (PlayerInputIsDisabled) return;

        if (!PlayerInputIsDisabled)
        {
            ResetAnimationTriggers();
            PlayerMovementInput();
            PlayerWalkInput();
        }


    }

    // 移动必须放在 FixedUpdate
    private void FixedUpdate()
    {
        PlayerMovement();
        EventHandler.CallMovementEvent(parameters);
    }

    private void PlayerMovement()
    {
        Vector2 inputDir = new Vector2(parameters.inputX, parameters.inputY).normalized;
        Vector2 move = inputDir * movementSpeed * Time.fixedDeltaTime;
        Vector2 targetPos = rb.position + move;

        //// 像素对齐
        //targetPos.x = Mathf.Round(targetPos.x / PIXEL_STEP) * PIXEL_STEP;
        //targetPos.y = Mathf.Round(targetPos.y / PIXEL_STEP) * PIXEL_STEP;

        rb.MovePosition(targetPos);
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
}