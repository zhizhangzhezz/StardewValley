using UnityEngine;

// 优化后的委托 → 只传一个打包参数
public delegate void MovementDelegate(MovementEventParams parameters);

public static class EventHandler
{
    // 运动事件
    public static event MovementDelegate MovementEvent;

    public static void CallMovementEvent(MovementEventParams parameters)
    {
        MovementEvent?.Invoke(parameters);
    }
}