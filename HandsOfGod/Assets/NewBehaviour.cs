using UnityEngine;
using UnityEngine.AI; // ʹ��Unity�ĵ���ϵͳ

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))] // 3D��ײ��
public class ZombieAI3D : MonoBehaviour
{
    [Header("Tracking Settings/׷������")]
    [Tooltip("Detection radius (meters)/���뾶(��)")]
    public float detectionRadius = 10f; // ����ֵ��5-15��

    [Tooltip("Movement speed/�ƶ��ٶ�")]
    public float moveSpeed = 3.5f; // ����ֵ��2-5

    [Header("Attack Settings/��������")]
    [Tooltip("Attack range/������Χ")]
    public float attackRange = 1.5f; // ƥ�乥��������Χ

    [Tooltip("Attack angle/�����Ƕ�")]
    [Range(0, 180)] public float attackAngle = 45f; // ǰ����������

    [Tooltip("Attack cooldown/������ȴ")]
    public float attackCooldown = 2f; // ��

    [Tooltip("Knockback force/��������")]
    public float knockbackForce = 5f; // ����ֵ��3-10

    [Tooltip("Attack damage/�����˺�")]
    public int damage = 10;

    [Header("References/����")]
    [Tooltip("Player transform/���Transform")]
    public Transform player;

    [Tooltip("Attack trigger name/��������������")]
    public string attackAnimTrigger = "Attack";

    [Tooltip("Movement parameter/�ƶ�����")]
    public string moveAnimParam = "Speed";

    [Tooltip("Attack sound/������Ч")]
    public AudioClip attackSound;

    [Tooltip("Attack hitbox/������ײ��")]
    public BoxCollider attackHitbox; // ר�����ڹ����ж�����ײ��

    // ˽�б���
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

        // ��ʼ��NavMeshAgent
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f; // ͣ�ڹ�����Χ��һ��
        agent.angularSpeed = 120f; // ��ת�ٶ�

        // ȷ��������ײ���ʼ����
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // ��Ұ���
        isPlayerInSight = false;
        if (distance <= detectionRadius)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle <= attackAngle * 0.5f)
            {
                // ���߼����⴩ǽ
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

        // ��Ϊ�߼�
        if (isPlayerInSight)
        {
            if (distance > attackRange)
            {
                // ׷��״̬
                agent.isStopped = false;
                agent.SetDestination(player.position);
                animator.SetFloat(moveAnimParam, agent.velocity.magnitude);
            }
            else
            {
                // ����״̬
                agent.isStopped = true;
                animator.SetFloat(moveAnimParam, 0);

                // �������
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
            // ����״̬
            agent.isStopped = true;
            animator.SetFloat(moveAnimParam, 0);
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;

        // ��������
        animator.SetTrigger(attackAnimTrigger);

        // ������Ч
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        // �������ù�����ײ��
        if (attackHitbox != null)
        {
            attackHitbox.enabled = true;
            Invoke("DisableHitbox", 0.5f); // ƥ�䶯��ʱ��
        }
    }

    void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
        isAttacking = false;
    }

    // ������ײ�崥�����
    void OnTriggerEnter(Collider other)
    {
        if (!isAttacking || other.transform != player) return;

        // Ӧ�û���
        Vector3 knockbackDir = (player.position - transform.position).normalized;
        knockbackDir.y = 0.3f; // ��΢����

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }

        // ���Կ��ӻ�
        void OnDrawGizmosSelected()
        {
            // ��ⷶΧ
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // ������Χ
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // ��Ұ�Ƕ�
            Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle * 0.5f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, attackAngle * 0.5f, 0) * transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, leftBoundary * detectionRadius);
            Gizmos.DrawRay(transform.position, rightBoundary * detectionRadius);
        }
    }
}