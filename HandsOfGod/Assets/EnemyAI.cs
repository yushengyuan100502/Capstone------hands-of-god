using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyType
    {
        Mone,       // Basic melee attacker
        Cairo,      // Fast mover
        Talk,       // Stationary shooter
        Ertry,      // Area attacker
        Stunted,    // Small but numerous
        Julero,     // Jumping attacker
        Specialkrtles, // Special abilities
        Death       // Strong final enemy
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
    public Projectile projectilePrefab; // For ranged enemies

    private Transform player;
    private Animator animator;
    private float lastAttackTime;
    private float lastAbilityTime;
    private bool isFacingRight = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Initialize enemy based on type
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
                moveSpeed = 0f; // Stationary
                attackRange = 5f; // Ranged attack
                break;
            case EnemyType.Ertry:
                attackRadius = 1.5f; // Larger attack area
                break;
            case EnemyType.Stunted:
                transform.localScale *= 0.7f;
                attackDamage = 5;
                break;
            case EnemyType.Julero:
                // This enemy would need a jump component added
                break;
            case EnemyType.Specialkrtles:
                // Special abilities handled in update
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

        // Handle facing direction
        UpdateFacingDirection();

        if (distance <= attackRange)
        {
            // Attack behavior based on enemy type
            HandleAttackBehavior();
            
            // Stop moving for most enemies (except maybe some special cases)
            if (enemyType != EnemyType.Specialkrtles)
            {
                animator.SetBool("isMoving", false);
            }
        }
        else
        {
            // Movement behavior based on enemy type
            HandleMovementBehavior(distance);
        }

        // Special abilities for certain enemies
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
    }

    void HandleAttackBehavior()
    {
        switch (enemyType)
        {
            case EnemyType.Talk: // Ranged attacker
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    animator.SetTrigger("AttackTrigger");
                    lastAttackTime = Time.time;
                }
                break;
                
            default: // Melee attacker
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    animator.SetTrigger("AttackTrigger");
                    lastAttackTime = Time.time;
                }
                break;
        }
    }

    void HandleMovementBehavior(float distance)
    {
        switch (enemyType)
        {
            case EnemyType.Talk: // Stationary
                animator.SetBool("isMoving", false);
                break;
                
            case EnemyType.Julero: // Jumping movement
                // Implement jumping logic here
                // Move towards player while jumping
                animator.SetBool("isMoving", true);
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                break;
                
            default: // Standard movement
                animator.SetBool("isMoving", true);
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                break;
        }
    }

    void HandleSpecialAbilities(float distance)
    {
        if (enemyType == EnemyType.Specialkrtles && Time.time >= lastAbilityTime + specialAbilityCooldown)
        {
            // Example special ability - dash attack
            if (distance > attackRange && distance < attackRange * 3)
            {
                animator.SetTrigger("SpecialAbility");
                lastAbilityTime = Time.time;
            }
        }
    }

    // Called from animation event for melee attacks
    // 在 EnemyAI 脚本中修改 DealDamage 方法
    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (var hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    // Called from animation event for ranged attacks
    public void FireProjectile()
    {
        if (projectilePrefab != null)
        {
            Projectile projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
            Vector2 direction = (player.position - attackPoint.position).normalized;
            projectile.SetDirection(direction);
        }
    }

    // Called from animation event for special abilities
    public void ExecuteSpecialAbility()
    {
        // Example dash ability for Specialkrtles
        if (enemyType == EnemyType.Specialkrtles)
        {
            Vector2 dashDirection = (player.position - transform.position).normalized;
            transform.position += (Vector3)(dashDirection * attackRange * 1.5f);
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