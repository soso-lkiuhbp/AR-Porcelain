using UnityEngine;

public class ModelTouchControl : MonoBehaviour
{
    [Header("是否允许操作")]
    public bool enableControl = true;

    [Header("旋转")]
    public float rotationSpeed = 0.2f;

    [Header("缩放")]
    public float zoomSpeed = 0.01f;
    public float minScale = 0.2f;
    public float maxScale = 100f;

    private Vector2 lastMousePos;

    void Update()
    {
        if (!enableControl) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    /* ===================== 手机触控 ===================== */
    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Rotate(touch.deltaPosition);
            }
        }
        else if (Input.touchCount == 2)
        {
            PinchZoom();
        }
    }

    /* ===================== Editor / PC ===================== */
    void HandleMouseInput()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePos = Input.mousePosition;
            }
            else
            {
                Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
                Rotate(delta);
                lastMousePos = Input.mousePosition;
            }
        }

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel != 0)
        {
            ApplyZoom(wheel * 200f);
        }
    }

    /* ===================== 核心：全方位旋转 ===================== */
    void Rotate(Vector2 delta)
    {
        // X 方向 → 绕 Y 轴旋转
        float yaw = -delta.x * rotationSpeed;

        // Y 方向 → 绕 X 轴旋转（上下翻）
        float pitch = delta.y * rotationSpeed;

        transform.Rotate(pitch, yaw, 0, Space.World);
    }

    /* ===================== 缩放 ===================== */
    void PinchZoom()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        Vector2 prevT0 = t0.position - t0.deltaPosition;
        Vector2 prevT1 = t1.position - t1.deltaPosition;

        float prevDist = Vector2.Distance(prevT0, prevT1);
        float currDist = Vector2.Distance(t0.position, t1.position);

        ApplyZoom(currDist - prevDist);
    }

    void ApplyZoom(float deltaDistance)
    {
        float newScale = Mathf.Clamp(
            transform.localScale.z + deltaDistance * zoomSpeed,
            minScale,
            maxScale
        );

        transform.localScale = new Vector3(newScale, newScale, newScale);
    }
}