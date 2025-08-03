using UnityEngine;
using UnityEngine.AI; // 使用Unity的导航系统

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))] // 3D碰撞体
public class ZombieAI3D : MonoBehaviour
{
    [Header("Tracking Settings/追踪设置")]
    [Tooltip("Detection radius (meters)/检测半径(米)")]
    public float detectionRadius = 10f; // 建议值：5-15米

    [Tooltip("Movement speed/移动速度")]
    public float moveSpeed = 3.5f; // 建议值：2-5

    [Header("Attack Settings/攻击设置")]
    [Tooltip("Attack range/攻击范围")]
    public float attackRange = 1.5f; // 匹配攻击动画范围

    [Tooltip("Attack angle/攻击角度")]
    [Range(0, 180)] public float attackAngle = 45f; // 前方扇形区域

    [Tooltip("Attack cooldown/攻击冷却")]
    public float attackCooldown = 2f; // 秒

    [Tooltip("Knockback force/击退力度")]
    public float knockbackForce = 5f; // 建议值：3-10

    [Tooltip("Attack damage/攻击伤害")]
    public int damage = 10;

    [Header("References/引用")]
    [Tooltip("Player transform/玩家Transform")]
    public Transform player;

    [Tooltip("Attack trigger name/攻击触发器名称")]
    public string attackAnimTrigger = "Attack";

    [Tooltip("Movement parameter/移动参数")]
    public string moveAnimParam = "Speed";

    [Tooltip("Attack sound/攻击音效")]
    public AudioClip attackSound;

    [Tooltip("Attack hitbox/攻击碰撞箱")]
    public BoxCollider attackHitbox; // 专门用于攻击判定的碰撞体

    // 私有变量
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private float lastAttackTime;
    private bool isAttacking;
    private bool isPlayerInSight;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // 初始化NavMeshAgent
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f; // 停在攻击范围外一点
        agent.angularSpeed = 120f; // 旋转速度

        // 确保攻击碰撞体初始禁用
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // 视野检测
        isPlayerInSight = false;
        if (distance <= detectionRadius)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle <= attackAngle * 0.5f)
            {
                // 射线检测避免穿墙
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up,
                                  directionToPlayer, out hit, detectionRadius))
                {
                    if (hit.transform == player)
                    {
                        isPlayerInSight = true;
                    }
                }
            }
        }

        // 行为逻辑
        if (isPlayerInSight)
        {
            if (distance > attackRange)
            {
                // 追踪状态
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetFloat(moveAnimParam, agent.velocity.magnitude);
            }
            else
            {
                // 攻击状态
                agent.isStopped = true;
                animator.SetFloat(moveAnimParam, 0);

                // 朝向玩家
                Quaternion lookRotation = Quaternion.LookRotation(
                    new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, lookRotation, Time.deltaTime * 5f);

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack();
                }
            }
        }
        else
        {
            // 闲置状态
            agent.isStopped = true;
            animator.SetFloat(moveAnimParam, 0);
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        // 触发动画
        animator.SetTrigger(attackAnimTrigger);

        // 播放音效
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        // 短暂启用攻击碰撞体
        if (attackHitbox != null)
        {
            attackHitbox.enabled = true;
            Invoke("DisableHitbox", 0.5f); // 匹配动画时间
        }
    }

    void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
        isAttacking = false;
    }

    // 攻击碰撞体触发检测
    void OnTriggerEnter(Collider other)
    {
        if (!isAttacking || other.transform != player) return;

        // 应用击退
        Vector3 knockbackDir = (player.position - transform.position).normalized;
        knockbackDir.y = 0.3f; // 轻微向上

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }

        // 调试可视化
        void OnDrawGizmosSelected()
        {
            // 检测范围
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // 攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 视野角度
            Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle * 0.5f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, attackAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, leftBoundary * detectionRadius);
            Gizmos.DrawRay(transform.position, rightBoundary * detectionRadius);
        }
    }
}