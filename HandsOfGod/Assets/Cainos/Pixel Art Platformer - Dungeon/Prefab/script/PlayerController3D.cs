using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController3D : MonoBehaviour
{
    [Header("�ƶ�����")]
    [SerializeField] private float moveSpeed = 5f;      // �ƶ��ٶ�
    [SerializeField] private float jumpForce = 8f;      // ��Ծ����
    [SerializeField] private float gravityMultiplier = 2f; // ������������
    [SerializeField] private float airControl = 0.5f;   // ���п���ϵ��

    [Header("������")]
    [SerializeField] private Transform groundCheck;     // �������
    [SerializeField] private float checkRadius = 0.2f;   // ���뾶
    [SerializeField] private LayerMask groundLayer;     // ����ͼ��

    private Rigidbody rb;
    private bool isGrounded;           // �Ƿ��ڵ���
    private bool isFacingRight = true;  // �Ƿ��泯��
    private Vector3 moveDirection;      // �ƶ�����
    private float originalGravity;      // ԭʼ����ֵ

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalGravity = Physics.gravity.y;
    }

    void Update()
    {
        // ����Ƿ��ڵ���
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundLayer);

        // ��Ծ����
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }

        // ��������
        if (Input.GetKey(KeyCode.S) && !isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, -jumpForce * 0.6f, rb.velocity.z);
        }

        // ���õ���Ծ����Ч��
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // ��ȡ����
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // �����ƶ����򣨹̶�Z��Ϊ0��
        moveDirection = new Vector3(horizontalInput, 0, 0).normalized;

        // Ӧ���ƶ����ڵ���ʱ����ȫ���ƣ��ڿ���ʱ���Ƽ�����
        float currentControl = isGrounded ? 1f : airControl;
        Vector3 targetVelocity = moveDirection * moveSpeed * currentControl;
        targetVelocity.y = rb.velocity.y; // ����Y���ٶȲ���

        // Ӧ���ٶȣ�����Z��λ�ò��䣩
        rb.velocity = new Vector3(targetVelocity.x, targetVelocity.y, 0);

        // ��ɫ����ת
        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            Flip();
        }
    }

    // ��ת��ɫ����
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ���ӻ������ⷶΧ
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }

    // ȷ��Z��̶�
    void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}