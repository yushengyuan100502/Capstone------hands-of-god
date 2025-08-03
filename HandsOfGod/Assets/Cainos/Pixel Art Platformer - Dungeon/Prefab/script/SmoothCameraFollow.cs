using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmoothCamera2DFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;  // 要跟随的目标（玩家角色）

    [Header("基础设置")]
    public Vector2 offset = new Vector2(0f, 0f);  // 摄像机偏移
    public float smoothTime = 0.25f;  // 平滑时间（值越大越平滑）

    [Header("跟随模式")]
    public bool followX = true;  // 是否跟随X轴
    public bool followY = true;  // 是否跟随Y轴

    [Header("边界限制")]
    public bool useBounds = false;  // 是否使用边界限制
    public Rect levelBounds;  // 关卡边界（世界坐标）

    [Header("超前跟随（Look Ahead）")]
    public bool lookAhead = true;  // 是否启用超前跟随
    public float lookAheadFactor = 0.5f;  // 超前量系数
    public float lookAheadSmooth = 5f;  // 超前平滑度

    [Header("边缘缓冲")]
    public float edgeBufferX = 1.5f;  // X轴边缘缓冲距离
    public float edgeBufferY = 1f;  // Y轴边缘缓冲距离

    private Vector3 _currentVelocity;
    private Vector3 _lookAheadPos;
    private float _lookAheadDirection;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (target == null) return;

        // 计算超前跟随
        if (lookAhead)
        {
            // 检测玩家输入方向
            float xInput = Input.GetAxisRaw("Horizontal");

            if (Mathf.Abs(xInput) > 0.01f)
            {
                _lookAheadDirection = Mathf.Sign(xInput);
            }

            // 计算超前位置
            _lookAheadPos = Vector3.Lerp(
                _lookAheadPos,
                Vector3.right * (_lookAheadDirection * lookAheadFactor),
                lookAheadSmooth * Time.deltaTime);
        }
        else
        {
            _lookAheadPos = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        Vector3 targetPos = target.position + (Vector3)offset + _lookAheadPos;

        // 应用轴跟随限制
        if (!followX) targetPos.x = transform.position.x;
        if (!followY) targetPos.y = transform.position.y;

        // 保持Z轴不变
        targetPos.z = transform.position.z;

        // 应用平滑阻尼
        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, smoothTime);

        // 应用边缘缓冲
        newPos = ApplyEdgeBuffering(newPos);

        // 应用边界限制
        if (useBounds)
        {
            newPos = ClampToBounds(newPos);
        }

        transform.position = newPos;
    }

    private Vector3 ApplyEdgeBuffering(Vector3 position)
    {
        // 计算摄像机视口大小（世界单位）
        float camHeight = _camera.orthographicSize * 2;
        float camWidth = camHeight * _camera.aspect;

        // 计算缓冲区域
        float minX = target.position.x - (camWidth / 2 - edgeBufferX);
        float maxX = target.position.x + (camWidth / 2 - edgeBufferX);
        float minY = target.position.y - (camHeight / 2 - edgeBufferY);
        float maxY = target.position.y + (camHeight / 2 - edgeBufferY);

        // 应用缓冲
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        // 计算摄像机视口大小（世界单位）
        float camHeight = _camera.orthographicSize * 2;
        float camWidth = camHeight * _camera.aspect;

        // 限制摄像机位置不超出边界
        position.x = Mathf.Clamp(position.x,
                                levelBounds.xMin + camWidth / 2,
                                levelBounds.xMax - camWidth / 2);
        position.y = Mathf.Clamp(position.y,
                                levelBounds.yMin + camHeight / 2,
                                levelBounds.yMax - camHeight / 2);

        return position;
    }

    // 在Scene视图中绘制边界（仅编辑器可见）
    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(levelBounds.center, levelBounds.size);
        }
    }
}