using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;  // 要跟随的目标（玩家）
    public Vector3 offset = new Vector3(0, 0, -10);  // 偏移量

    void LateUpdate()
    {
        if (target != null)
        {
            // 最简单直接的跟随：每帧把相机位置设置成玩家位置+偏移
            transform.position = target.position + offset;
        }
    }
}