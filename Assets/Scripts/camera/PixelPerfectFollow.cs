using UnityEngine;
using UnityEngine.U2D;

public class PixelPerfectFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    private PixelPerfectCamera pixelPerfectCamera;
    private PolygonCollider2D boundsConfiner;

    void Awake()
    {
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
    }

    void Start()
    {
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

        Vector3 targetPos = target.position + offset;

        Vector3 finalPos = pixelPerfectCamera.RoundToPixel(targetPos);

        finalPos = ClampCameraToBounds(finalPos);

        transform.position = finalPos;
    }

    private Vector3 ClampCameraToBounds(Vector3 targetPos)
    {
        Bounds bounds = boundsConfiner.bounds;

        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = Camera.main.aspect * halfHeight;

        float clampedX = Mathf.Clamp(targetPos.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
        float clampedY = Mathf.Clamp(targetPos.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }
}
