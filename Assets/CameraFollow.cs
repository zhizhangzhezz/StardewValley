using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(PixelPerfectCamera))]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    [Range(1, 32)] public int pixelsPerUnit = 16;
    public float dampingX = 5f;
    public float dampingY = 5f;

    private PixelPerfectCamera _ppc;

    void Awake()
    {
        _ppc = GetComponent<PixelPerfectCamera>();
        _ppc.assetsPPU = pixelsPerUnit;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float ppu = pixelsPerUnit;

        float snappedX = Mathf.Round((target.position.x + offset.x) * ppu) / ppu;
        float snappedY = Mathf.Round((target.position.y + offset.y) * ppu) / ppu;

        float newX = Mathf.Lerp(transform.position.x, snappedX, dampingX * Time.deltaTime);
        float newY = Mathf.Lerp(transform.position.y, snappedY, dampingY * Time.deltaTime);

        transform.position = new Vector3(
            Mathf.Round(newX * ppu) / ppu,
            Mathf.Round(newY * ppu) / ppu,
            transform.position.z
        );
    }
}