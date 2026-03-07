using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTest : MonoBehaviour
{
    public MovementEventParams parameters;

    private void Update()
    {
        Debug.Log("正在发送事件");
        EventHandler.CallMovementEvent(parameters);
    }
}
