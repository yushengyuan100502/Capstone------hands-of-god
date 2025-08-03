using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyType
    {
        Mone, Cairo, Talk, Ertry, Stunted, Julero, Specialkrtles, Death
    }

    public EnemyType enemyType;
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public int attackDamage = 10;
    public Transform attackPoint;
    public float attackRadius = 0.7f;
    public LayerMask playerLayer;
    public float specialAbilityCooldown = 5f;
    public Projectile projectilePrefab;

    private Transform player;
    private Animator animator;
    private float lastAttackTime;
    private float lastAbilityTime;
    private bool isFacingRight = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        Debug.Log($"[Init] Enemy Type: {enemyType}");

        InitializeEnemyType();
    }

    void InitializeEnemyType()
    {
        switch (enemyType)
        {
            case EnemyType.Mone:
                moveSpeed = 50f;
                attackDamage = 15;
                break;
            case EnemyType.Cairo:
                moveSpeed = 4f;
                attackDamage = 8;
                break;
            case EnemyType.Talk:
                moveSpeed = 0f;
                attackRange = 5f;
                break;
            case EnemyType.Ertry:
                attackRadius = 1.5f;
                break;
            case EnemyType.Stunted:
                transform.localScale *= 0.7f;
                attackDamage = 5;
                break;
            case EnemyType.Death:
                moveSpeed = 1.5f;
                attackDamage = 25;
                GetComponent<PlayerHealth>().maxHealth *= 3;
                break;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        UpdateFacingDirection();

        if (distance <= attackRange)
        {
            Debug.Log($"[Combat] {enemyType} is attacking (distance {distance:F2})");

            HandleAttackBehavior();

            if (enemyType != EnemyType.Specialkrtles)
            {
                animator.SetBool("isMoving", false);
            }
        }
        else
        {
            HandleMovementBehavior(distance);
        }

        HandleSpecialAbilities(distance);
    }

    void UpdateFacingDirection()
    {
        if (player.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
        else if (player.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        Debug.Log($"[Flip] Facing {(isFacingRight ? "right" : "left")}");
    }

    void HandleAttackBehavior()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        switch (enemyType)
        {
            case EnemyType.Talk:
                Debug.Log("[Attack] Ranged attack triggered");
                animator.SetTrigger("AttackTrigger");
                break;

            default:
                Debug.Log("[Attack] Melee attack triggered");
                animator.SetTrigger("AttackTrigger");
                break;
        }

        lastAttackTime = Time.time;
    }

    void HandleMovementBehavior(float distance)
    {
        switch (enemyType)
        {
            case EnemyType.Talk:
                animator.SetBool("isMoving", false);
                break;

            case EnemyType.Julero:
                animator.SetBool("isMoving", true);
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                Debug.Log($"[Move] Julero jumping toward player (distance {distance:F2})");
                break;

            default:
                animator.SetBool("isMoving", true);
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                Debug.Log($"[Move] {enemyType} moving toward player (distance {distance:F2})");
                break;
        }
    }

    void HandleSpecialAbilities(float distance)
    {
        if (enemyType == EnemyType.Specialkrtles && Time.time >= lastAbilityTime + specialAbilityCooldown)
        {
            if (distance > attackRange && distance < attackRange * 3)
            {
                Debug.Log("[Special] Executing special ability (dash)");
                animator.SetTrigger("SpecialAbility");
                lastAbilityTime = Time.time;
            }
        }
    }

    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (var hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($"[Damage] Dealt {attackDamage} damage to Player");
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void FireProjectile()
    {
        if (projectilePrefab != null)
        {
            Projectile projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
            Vector2 direction = (player.position - attackPoint.position).normalized;
            projectile.SetDirection(direction);
            Debug.Log("[Projectile] Fired projectile toward player");
        }
    }

    public void ExecuteSpecialAbility()
    {
        if (enemyType == EnemyType.Specialkrtles)
        {
            Vector2 dashDirection = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dashDirection * attackRange * 1.5f);
            Debug.Log("[Special] Dashed toward player");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}