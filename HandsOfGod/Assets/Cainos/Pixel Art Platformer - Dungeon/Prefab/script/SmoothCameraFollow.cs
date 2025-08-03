using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmoothCamera2DFollow : MonoBehaviour
{
    [Header("����Ŀ��")]
    public Transform target;  // Ҫ�����Ŀ�꣨��ҽ�ɫ��

    [Header("��������")]
    public Vector2 offset = new Vector2(0f, 0f);  // �����ƫ��
    public float smoothTime = 0.25f;  // ƽ��ʱ�䣨ֵԽ��Խƽ����

    [Header("����ģʽ")]
    public bool followX = true;  // �Ƿ����X��
    public bool followY = true;  // �Ƿ����Y��

    [Header("�߽�����")]
    public bool useBounds = false;  // �Ƿ�ʹ�ñ߽�����
    public Rect levelBounds;  // �ؿ��߽磨�������꣩

    [Header("��ǰ���棨Look Ahead��")]
    public bool lookAhead = true;  // �Ƿ����ó�ǰ����
    public float lookAheadFactor = 0.5f;  // ��ǰ��ϵ��
    public float lookAheadSmooth = 5f;  // ��ǰƽ����

    [Header("��Ե����")]
    public float edgeBufferX = 1.5f;  // X���Ե�������
    public float edgeBufferY = 1f;  // Y���Ե�������

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

        // ���㳬ǰ����
        if (lookAhead)
        {
            // ���������뷽��
            float xInput = Input.GetAxisRaw("Horizontal");

            if (Mathf.Abs(xInput) > 0.01f)
            {
                _lookAheadDirection = Mathf.Sign(xInput);
            }

            // ���㳬ǰλ��
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

        // ����Ŀ��λ��
        Vector3 targetPos = target.position + (Vector3)offset + _lookAheadPos;

        // Ӧ�����������
        if (!followX) targetPos.x = transform.position.x;
        if (!followY) targetPos.y = transform.position.y;

        // ����Z�᲻��
        targetPos.z = transform.position.z;

        // Ӧ��ƽ������
        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, smoothTime);

        // Ӧ�ñ�Ե����
        newPos = ApplyEdgeBuffering(newPos);

        // Ӧ�ñ߽�����
        if (useBounds)
        {
            newPos = ClampToBounds(newPos);
        }

        transform.position = newPos;
    }

    private Vector3 ApplyEdgeBuffering(Vector3 position)
    {
        // ����������ӿڴ�С�����絥λ��
        float camHeight = _camera.orthographicSize * 2;
        float camWidth = camHeight * _camera.aspect;

        // ���㻺������
        float minX = target.position.x - (camWidth / 2 - edgeBufferX);
        float maxX = target.position.x + (camWidth / 2 - edgeBufferX);
        float minY = target.position.y - (camHeight / 2 - edgeBufferY);
        float maxY = target.position.y + (camHeight / 2 - edgeBufferY);

        // Ӧ�û���
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        // ����������ӿڴ�С�����絥λ��
        float camHeight = _camera.orthographicSize * 2;
        float camWidth = camHeight * _camera.aspect;

        // ���������λ�ò������߽�
        position.x = Mathf.Clamp(position.x,
                                levelBounds.xMin + camWidth / 2,
                                levelBounds.xMax - camWidth / 2);
        position.y = Mathf.Clamp(position.y,
                                levelBounds.yMin + camHeight / 2,
                                levelBounds.yMax - camHeight / 2);

        return position;
    }

    // ��Scene��ͼ�л��Ʊ߽磨���༭���ɼ���
    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(levelBounds.center, levelBounds.size);
        }
    }
}