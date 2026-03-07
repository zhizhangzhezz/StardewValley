using UnityEngine;
using UnityEngine.U2D;

public class PixelPerfectFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private PixelPerfectCamera pixelPerfectCamera;
    private PolygonCollider2D boundsConfiner;  // 地图边界

    void Awake()
    {
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        
    }

    // 改为在Start()里查找，此时所有场景都已加载
    void Start()
    {
        // 安全查找，避免空引用
        GameObject confinerObj = GameObject.FindGameObjectWithTag("BoundsConfiner");
        if (confinerObj != null)
        {
            boundsConfiner = confinerObj.GetComponent<PolygonCollider2D>();
        }
        else
        {
            Debug.LogError("找不到标签为BoundsConfiner的物体！");
        }
    }

    void LateUpdate()
    {
        if (target == null || boundsConfiner == null) return;

        // 1. 计算目标位置
        Vector3 targetPos = target.position + offset;

        // 2. 像素对齐
        Vector3 finalPos = pixelPerfectCamera.RoundToPixel(targetPos);

        // 3. 限制相机在地图边界内
        finalPos = ClampCameraToBounds(finalPos);

        // 4. 应用位置
        transform.position = finalPos;
    }

    // 相机限制在 BoundsConfiner 范围内
    private Vector3 ClampCameraToBounds(Vector3 targetPos)
    {
        Bounds bounds = boundsConfiner.bounds;

        // 相机一半宽高
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = Camera.main.aspect * halfHeight;

        // 限制 X 和 Y
        float clampedX = Mathf.Clamp(targetPos.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
        float clampedY = Mathf.Clamp(targetPos.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }
}