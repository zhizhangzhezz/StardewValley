using UnityEngine;

public class CinemachineDampingFix : MonoBehaviour
{
    [Header("相机跟随设置")]
    [SerializeField] private Transform player;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    [Header("边界设置")]
    [SerializeField] private PolygonCollider2D boundsConfiner;

    private Camera mainCamera;
    private Vector3 velocity = Vector3.zero;
    private bool initialized = false;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CinemachineDampingFix: 找不到主相机！");
            return;
        }

        // 查找玩家
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("CinemachineDampingFix: 自动找到玩家: " + player.name);
            }
            else
            {
                Debug.LogError("CinemachineDampingFix: 找不到标签为 'Player' 的物体！请在 Inspector 中手动设置 Player。");
                return;
            }
        }

        // 查找边界
        if (boundsConfiner == null)
        {
            GameObject confinerObj = GameObject.FindGameObjectWithTag("BoundsConfiner");
            if (confinerObj != null)
            {
                boundsConfiner = confinerObj.GetComponent<PolygonCollider2D>();
                if (boundsConfiner != null)
                {
                    Debug.Log("CinemachineDampingFix: 自动找到边界: " + boundsConfiner.name);
                }
            }
        }
    }

    void Start()
    {
        if (player == null || mainCamera == null)
        {
            Debug.LogError("CinemachineDampingFix: 初始化失败，Player 或 MainCamera 为空！");
            return;
        }

        // 初始化相机位置
        Vector3 initialPos = player.position + offset;
        if (boundsConfiner != null)
        {
            initialPos = ClampToBounds(initialPos);
        }
        mainCamera.transform.position = initialPos;
        velocity = Vector3.zero;
        initialized = true;

        Debug.Log("CinemachineDampingFix: 初始化完成，相机位置: " + initialPos);
    }

    void LateUpdate()
    {
        if (!initialized || player == null || mainCamera == null) return;

        Vector3 targetPos = player.position + offset;

        // 边界限制
        if (boundsConfiner != null)
        {
            targetPos = ClampToBounds(targetPos);
        }

        // 平滑跟随
        Vector3 smoothedPos = Vector3.SmoothDamp(mainCamera.transform.position, targetPos, ref velocity, smoothTime);

        mainCamera.transform.position = smoothedPos;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (boundsConfiner == null) return position;

        Bounds bounds = boundsConfiner.bounds;
        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = mainCamera.aspect * halfHeight;

        // 如果边界太小，不进行限制
        if (bounds.size.x <= halfWidth * 2 || bounds.size.y <= halfHeight * 2)
        {
            return position;
        }

        float clampedX = Mathf.Clamp(position.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
        float clampedY = Mathf.Clamp(position.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

        return new Vector3(clampedX, clampedY, position.z);
    }
}
