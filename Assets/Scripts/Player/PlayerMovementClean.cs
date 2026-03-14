using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementClean : MonoBehaviour
{
    [Header("移动速度")]
    public float moveSpeed = 5.333f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        //rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 🔥 关键：防抖动
    }

    private void Update()
    {
        // 获取输入
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        // 移动
        Vector2 movePos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(movePos);
    }
}