using UnityEngine;

//  movement 所有参数
[System.Serializable]
public struct MovementEventParams
{
    // 输入
    public float inputX;
    public float inputY;

    // 状态
    public bool isWalking;
    public bool isRunning;
    public bool isIdle;
    public bool isCarrying;

    // 工具效果
    public ToolEffect toolEffect;

    // 使用工具方向
    public bool isUsingToolRight;
    public bool isUsingToolLeft;
    public bool isUsingToolUp;
    public bool isUsingToolDown;

    // 举起工具方向
    public bool isLiftingToolRight;
    public bool isLiftingToolLeft;
    public bool isLiftingToolUp;
    public bool isLiftingToolDown;

    // 拾取方向
    public bool isPickingRight;
    public bool isPickingLeft;
    public bool isPickingUp;
    public bool isPickingDown;

    // 挥舞工具方向
    public bool isSwingingToolRight;
    public bool isSwingingToolLeft;
    public bool isSwingingToolUp;
    public bool isSwingingToolDown;

    // 空闲方向
    public bool idleUp;
    public bool idleDown;
    public bool idleLeft;
    public bool idleRight;
}