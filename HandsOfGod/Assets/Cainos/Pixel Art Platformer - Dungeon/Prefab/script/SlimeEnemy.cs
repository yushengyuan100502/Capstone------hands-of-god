using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float jumpForce = 4f;  // Reduced from 8f to make lower jumps
    public float moveSpeed = 5f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    
    [Header("Jump Settings")]
    public float jumpCooldown = 1.5f;  // Reduced from 2f for more responsive jumping
    public float maxJumpHeight = 3f;   // Maximum height the slime can jump
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayerMask = -1; // All layers by default
    
    [Header("Attack Settings")]
    public int damage = 20;
    public float attackCooldown = 1f;
    public int contactDamage = 15; // Damage when touching player
    
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    
    [Header("Knockback Settings")]
    public float knockbackForce = 4f;
    public float knockbackDuration = 0.3f;
    private bool isKnockedBack = false;
    
    [Header("AI Settings")]
    public float patrolDistance = 5f;
    public float idleTime = 2f;
    
    [Header("Enemy Spawning Settings")]
    public bool enableEnemySpawning = false;
    public GameObject enemyPrefabToSpawn;
    public int numberOfEnemiesToSpawn = 2;
    public float spawnRadius = 3f;
    public bool spawnOnDeath = true;
    public bool spawnOnAttack = false;
    public bool spawnOnInterval = false;
    public float spawnInterval = 10f; // Time between interval spawns
    private float spawnTimer = 0f;
    private bool hasSpawnedOnDeath = false;
    
    // Private variables
    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private GameObject player;
    
    // State variables
    private bool isGrounded = false;
    private bool canJump = true;
    private bool canAttack = true;
    private bool facingRight = true;
    private bool cooldownActive = false; // Prevents multiple cooldown coroutines
    
    // AI State
    private enum EnemyState { Idle, Chasing, Attacking, Patrolling }
    private EnemyState currentState = EnemyState.Idle;
    
    // Patrol variables
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private float idleTimer = 0f;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("SlimeEnemy: No player found with 'Player' tag!");
        }
        
        // Set up patrol
        startPosition = transform.position;
        SetNewPatrolTarget();
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Freeze Z rotation to keep it 2D
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
    }
    
    void Update()
    {
        CheckGrounded();
        UpdateAI();
        UpdateAnimation();
        UpdateSpawning();
    }
    
    void CheckGrounded()
    {
        // Check if slime is on ground using raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayerMask);
        
        // Reset jump ability when grounded
        if (isGrounded && rb.velocity.y <= 0.1f)
        {
            if (!canJump && !cooldownActive)
            {
                cooldownActive = true;
                StartCoroutine(JumpCooldownRoutine());
            }
        }
    }
    
    void UpdateAI()
    {
        if (player == null || isKnockedBack) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        
        // State machine
        switch (currentState)
        {
            case EnemyState.Idle:
                idleTimer += Time.deltaTime;
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                }
                else if (idleTimer >= idleTime)
                {
                    currentState = EnemyState.Patrolling;
                    idleTimer = 0f;
                }
                break;
                
            case EnemyState.Chasing:
                if (distanceToPlayer > detectionRange)
                {
                    currentState = EnemyState.Idle;
                }
                else if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attacking;
                }
                else
                {
                    ChasePlayer();
                }
                break;
                
            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chasing;
                }
                else
                {
                    AttackPlayer();
                }
                break;
                
            case EnemyState.Patrolling:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                }
                else
                {
                    Patrol();
                }
                break;
        }
    }
    
    void ChasePlayer()
    {
        // Calculate direction to player once
        Vector3 direction = (player.transform.position - transform.position).normalized;
        
        // Can't chase if not grounded or jump is on cooldown
        if (!isGrounded || !canJump) 
        {
            // If in air, just face the player but don't try to jump
            if (!isGrounded)
            {
                if (direction.x > 0 && !facingRight)
                {
                    Flip();
                }
                else if (direction.x < 0 && facingRight)
                {
                    Flip();
                }
            }
            return;
        }
        
        float heightDifference = player.transform.position.y - transform.position.y;
        
        // If player is too high to reach, give up chasing and return to patrol
        if (heightDifference > maxJumpHeight + 1f)
        {
            currentState = EnemyState.Patrolling;
            SetNewPatrolTarget();
            return;
        }
        
        // Face the player
        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && facingRight)
        {
            Flip();
        }
        
        // Jump towards player only if grounded and can jump
        JumpTowards(direction);
    }
    
    void AttackPlayer()
    {
        if (!canAttack) return;
        
        // Deal damage to player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("Slime attacked player for " + damage + " damage!");
        }
        
        // Start attack cooldown
        StartCoroutine(AttackCooldownRoutine());
        
        // Spawn enemies on attack if enabled
        if (enableEnemySpawning && spawnOnAttack)
        {
            SpawnEnemies();
        }
        
        // Jump back after attacking
        if (canJump && isGrounded)
        {
            Vector3 awayDirection = (transform.position - player.transform.position).normalized;
            JumpTowards(awayDirection * 0.5f); // Smaller jump backwards
        }
    }
    
    void Patrol()
    {
        // Can't patrol if not grounded or jump is on cooldown
        if (!isGrounded || !canJump) return;
        
        float distanceToPatrolTarget = Vector3.Distance(transform.position, patrolTarget);
        
        if (distanceToPatrolTarget <= 1f)
        {
            currentState = EnemyState.Idle;
            SetNewPatrolTarget();
            return;
        }
        
        Vector3 direction = (patrolTarget - transform.position).normalized;
        
        // Face patrol direction
        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && facingRight)
        {
            Flip();
        }
        
        // Jump towards patrol target only if grounded and can jump
        JumpTowards(direction * 0.7f); // Slower patrol movement
    }
    
    void JumpTowards(Vector3 direction)
    {
        if (!canJump || !isGrounded) return;
        
        // Calculate the target height difference
        float targetHeight = 0f;
        if (currentState == EnemyState.Chasing && player != null)
        {
            targetHeight = player.transform.position.y - transform.position.y;
        }
        
        // Limit jump force based on maximum jump height
        float actualJumpForce = jumpForce;
        if (targetHeight > maxJumpHeight)
        {
            // If target is too high, use maximum jump force but don't exceed max height
            actualJumpForce = Mathf.Sqrt(2f * Physics.gravity.magnitude * maxJumpHeight);
        }
        else if (targetHeight > 0)
        {
            // Calculate required jump force for the target height
            float requiredForce = Mathf.Sqrt(2f * Physics.gravity.magnitude * (targetHeight + 0.5f));
            actualJumpForce = Mathf.Min(requiredForce, jumpForce);
        }
        
        // Calculate jump vector
        Vector3 jumpVector = new Vector3(direction.x * moveSpeed, actualJumpForce, 0);
        
        // Apply jump force
        rb.velocity = jumpVector;
        
        // Start jump cooldown
        canJump = false;
        cooldownActive = false; // Reset flag so cooldown can start when landing
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = facingRight ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
    }
    
    void SetNewPatrolTarget()
    {
        float randomX = Random.Range(-patrolDistance, patrolDistance);
        patrolTarget = new Vector3(startPosition.x + randomX, startPosition.y, startPosition.z);
    }
    
    void UpdateAnimation()
    {
        if (animator == null) return;
        
        // Set animation parameters based on state
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isChasing", currentState == EnemyState.Chasing && canJump && isGrounded);
        animator.SetBool("isAttacking", currentState == EnemyState.Attacking);
        animator.SetBool("canAct", canJump && isGrounded); // New parameter for when slime can actually do something
        animator.SetFloat("velocityY", rb.velocity.y);
    }
    
    IEnumerator JumpCooldownRoutine()
    {
        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
        cooldownActive = false; // Reset cooldown flag
    }
    
    IEnumerator AttackCooldownRoutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    // Damage system for the enemy
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Slime took " + damageAmount + " damage! Health: " + currentHealth + "/" + maxHealth);
        
        // Apply knockback effect
        if (player != null)
        {
            Vector3 knockbackDirection = (transform.position - player.transform.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }
        
        // Check if enemy should die
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("Slime defeated!");
        
        // Trigger death animation
        if (animator != null)
        {
            animator.SetBool("die", true);
        }
        
        // Disable AI and movement while death animation plays
        currentState = EnemyState.Idle;
        
        // Spawn enemies on death if enabled and not already spawned
        if (enableEnemySpawning && spawnOnDeath && !hasSpawnedOnDeath)
        {
            hasSpawnedOnDeath = true;
            SpawnEnemies();
        }
        
        // Start death sequence
        StartCoroutine(DeathSequence());
    }
    
    IEnumerator DeathSequence()
    {
        // Wait for death animation to play (adjust time as needed)
        yield return new WaitForSeconds(1.0f);
        
        // Add death effects here (particles, sound, etc.)
        Destroy(gameObject);
    }
    
    IEnumerator ApplyKnockback(Vector3 direction)
    {
        isKnockedBack = true;
        
        // Apply knockback force
        rb.velocity = new Vector3(direction.x * knockbackForce, 0, 0);
        
        // Wait for knockback duration
        yield return new WaitForSeconds(knockbackDuration);
        
        isKnockedBack = false;
    }
    
    // Contact damage when slime touches player
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null && !isKnockedBack)
            {
                playerHealth.TakeDamage(contactDamage);
                Debug.Log("Slime touched player for " + contactDamage + " contact damage!");
            }
        }
    }
    
    // Enemy spawning methods
    void UpdateSpawning()
    {
        if (!enableEnemySpawning || !spawnOnInterval) return;
        
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemies();
            spawnTimer = 0f;
        }
    }
    
    void SpawnEnemies()
    {
        if (enemyPrefabToSpawn == null)
        {
            Debug.LogWarning("SlimeEnemy: No enemy prefab set for spawning!");
            return;
        }
        
        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            // Calculate random spawn position around the slime (same Z position)
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = new Vector3(
                transform.position.x + randomCircle.x, 
                transform.position.y, 
                transform.position.z  // Keep same Z position as slime
            );
            
            // Check if spawn position is valid (not inside walls, etc.)
            if (IsValidSpawnPosition(spawnPosition))
            {
                GameObject spawnedEnemy = Instantiate(enemyPrefabToSpawn, spawnPosition, Quaternion.identity);
                Debug.Log("Slime spawned enemy at position: " + spawnPosition);
            }
            else
            {
                // Try spawning at a different position
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    randomCircle = Random.insideUnitCircle * spawnRadius;
                    spawnPosition = new Vector3(
                        transform.position.x + randomCircle.x, 
                        transform.position.y, 
                        transform.position.z  // Keep same Z position as slime
                    );
                    
                    if (IsValidSpawnPosition(spawnPosition))
                    {
                        GameObject spawnedEnemy = Instantiate(enemyPrefabToSpawn, spawnPosition, Quaternion.identity);
                        Debug.Log("Slime spawned enemy at position: " + spawnPosition + " (attempt " + (attempt + 1) + ")");
                        break;
                    }
                }
            }
        }
    }
    
    bool IsValidSpawnPosition(Vector3 position)
    {
        // Check if there's ground beneath the spawn position
        bool hasGround = Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, 2f, groundLayerMask);
        
        // Check if the spawn position is not occupied by other objects
        Collider[] overlapping = Physics.OverlapSphere(position, 0.5f);
        bool isOccupied = false;
        
        foreach (Collider col in overlapping)
        {
            if (col.gameObject != gameObject && !col.isTrigger)
            {
                isOccupied = true;
                break;
            }
        }
        
        return hasGround && !isOccupied;
    }
    
    // Visual debugging
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Ground check
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        
        // Maximum jump height
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * maxJumpHeight);
        Gizmos.DrawWireCube(transform.position + Vector3.up * maxJumpHeight, new Vector3(1f, 0.1f, 1f));
        
        // Patrol area
        Gizmos.color = Color.blue;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireCube(startPosition, new Vector3(patrolDistance * 2, 1, 1));
            Gizmos.DrawSphere(patrolTarget, 0.5f);
        }
        
        // Enemy spawn radius
        if (enableEnemySpawning)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}