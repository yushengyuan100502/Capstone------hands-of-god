using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController3D : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;      // 移动速度
    [SerializeField] private float jumpForce = 8f;      // 跳跃力度
    [SerializeField] private float gravityMultiplier = 2f; // 下落重力倍数
    [SerializeField] private float airControl = 0.5f;   // 空中控制系数

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;     // 地面检测点
    [SerializeField] private float checkRadius = 0.2f;   // 检测半径
    [SerializeField] private LayerMask groundLayer;     // 地面图层

    private Rigidbody rb;
    private bool isGrounded;           // 是否在地面
    private bool isFacingRight = true;  // 是否面朝右
    private Vector3 moveDirection;      // 移动方向
    private float originalGravity;      // 原始重力值

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalGravity = Physics.gravity.y;
    }

    void Update()
    {
        // 检测是否在地面
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundLayer);

        // 跳跃输入
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }

        // 下落输入
        if (Input.GetKey(KeyCode.S) && !isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, -jumpForce * 0.6f, rb.velocity.z);
        }

        // 更好的跳跃物理效果
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1) * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // 获取输入
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // 计算移动方向（固定Z轴为0）
        moveDirection = new Vector3(horizontalInput, 0, 0).normalized;

        // 应用移动（在地面时有完全控制，在空中时控制减弱）
        float currentControl = isGrounded ? 1f : airControl;
        Vector3 targetVelocity = moveDirection * moveSpeed * currentControl;
        targetVelocity.y = rb.velocity.y; // 保持Y轴速度不变

        // 应用速度（保持Z轴位置不变）
        rb.velocity = new Vector3(targetVelocity.x, targetVelocity.y, 0);

        // 角色朝向翻转
        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            Flip();
        }
    }

    // 翻转角色朝向
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // 可视化地面检测范围
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }

    // 确保Z轴固定
    void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}