using UnityEngine;
using UnityEngine.U2D;

public class PixelPerfectCameraFollow : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] private Transform player;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    [Header("边界设置")]
    [SerializeField] private PolygonCollider2D boundsConfiner;

    private Camera mainCamera;
    private PixelPerfectCamera pixelPerfectCamera;
    private Vector3 velocity = Vector3.zero;
    private Vector3 internalPosition;  // 内部浮点位置（用于平滑计算）

    void Awake()
    {
        mainCamera = Camera.main;
        pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (boundsConfiner == null)
        {
            GameObject confinerObj = GameObject.FindGameObjectWithTag("BoundsConfiner");
            if (confinerObj != null) boundsConfiner = confinerObj.GetComponent<PolygonCollider2D>();
        }
    }

    void Start()
    {
        if (player != null)
        {
            internalPosition = player.position + offset;
            mainCamera.transform.position = internalPosition;
            velocity = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        if (player == null || mainCamera == null) return;

        // 目标位置 = 玩家位置 + 偏移
        Vector3 targetPos = player.position + offset;

        // 边界限制
        if (boundsConfiner != null)
        {
            targetPos = ClampToBounds(targetPos);
        }

        // 平滑跟随（使用内部浮点位置，丝滑！）
        internalPosition = Vector3.SmoothDamp(internalPosition, targetPos, ref velocity, smoothTime);

        // 只有当相机接近停止时才进行像素对齐
        if (velocity.sqrMagnitude < 0.001f)
        {
            // 相机几乎停止，进行像素对齐
            Vector3 pixelPerfectPos = pixelPerfectCamera != null
                ? pixelPerfectCamera.RoundToPixel(internalPosition)
                : internalPosition;
            mainCamera.transform.position = pixelPerfectPos;
        }
        else
        {
            // 相机正在移动，保持浮点精度（丝滑！）
            mainCamera.transform.position = internalPosition;
        }
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (boundsConfiner == null) return position;

        Bounds bounds = boundsConfiner.bounds;
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = mainCamera.aspect * halfHeight;

        if (bounds.size.x <= halfWidth * 2 || bounds.size.y <= halfHeight * 2)
            return position;

        float clampedX = Mathf.Clamp(position.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
        float clampedY = Mathf.Clamp(position.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

        return new Vector3(clampedX, clampedY, position.z);
    }
}
