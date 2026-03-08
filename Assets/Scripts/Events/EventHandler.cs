using UnityEngine;
using System;
using System.Collections.Generic;

// 优化后的委托 → 只传一个打包参数
public delegate void MovementDelegate(MovementEventParams parameters);

public static class EventHandler
{
    //更新库存事件
    public static event Action<InventoryLocation, List<InventoryItem>> InventoryUpdatedEvent;

    public static void CallInventoryUpdatedEvent(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
    {
        if(InventoryUpdatedEvent != null)
        {
            InventoryUpdatedEvent(inventoryLocation, inventoryList);
        }
    }

    // 运动事件
    public static event MovementDelegate MovementEvent;

    public static void CallMovementEvent(MovementEventParams parameters)
    {
        MovementEvent?.Invoke(parameters);
    }
}