using UnityEngine;
using UnityEngine.Events; // 用于事件系统

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings/血量设置")]
    [Tooltip("Max health/最大血量")]
    public int maxHealth = 100; // [可调整] 玩家初始血量

    [Tooltip("Current health/当前血量")]
    public int currentHealth; // 当前血量（会在Start中初始化）

    [Tooltip("Invincible time after hit/受伤无敌时间")]
    public float invincibleTime = 1f; // [可调整] 受伤后的无敌时间(秒)

    [Header("Effects/效果")]
    [Tooltip("Damage sound effect/受伤音效")]
    public AudioClip damageSound;

    [Tooltip("Death sound effect/死亡音效")]
    public AudioClip deathSound;

    [Tooltip("Damage visual effect/受伤视觉效果")]
    public Material damageMaterial; // 受伤时临时替换的材质
    private Material originalMaterial; // 原始材质

    [Header("Events/事件")]
    public UnityEvent onDamageTaken; // 受伤时触发的事件
    public UnityEvent onDeath; // 死亡时触发的事件

    // 私有变量
    private AudioSource audioSource;
    private Renderer playerRenderer;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    void Start()
    {
        // 初始化组件
        audioSource = GetComponent<AudioSource>();
        playerRenderer = GetComponentInChildren<Renderer>();

        // 存储原始材质
        if (playerRenderer != null)
        {
            originalMaterial = playerRenderer.material;
        }

        // 初始化血量
        currentHealth = maxHealth;
    }

    void Update()
    {
        // 无敌时间计时器
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0)
            {
                EndInvincibility();
            }
        }
    }

    // 受到伤害的公共方法（会被ZombieAI调用）
    public void TakeDamage(int damage)
    {
        // 如果处于无敌状态则忽略伤害
        if (isInvincible || currentHealth <= 0) return;

        // 扣除血量
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage! Current health: {currentHealth}");

        // 触发受伤事件
        onDamageTaken.Invoke();

        // 播放受伤音效
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // 应用受伤视觉效果
        if (damageMaterial != null && playerRenderer != null)
        {
            playerRenderer.material = damageMaterial;
        }

        // 开始无敌时间
        StartInvincibility();

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 击退效果（会被ZombieAI调用）
    public void ApplyKnockback(Vector3 direction, float force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    void StartInvincibility()
    {
        isInvincible = true;
        invincibleTimer = invincibleTime;

        // 可以在这里添加闪烁效果
        // StartCoroutine(FlashEffect());
    }

    void EndInvincibility()
    {
        isInvincible = false;

        // 恢复原始材质
        if (playerRenderer != null && originalMaterial != null)
        {
            playerRenderer.material = originalMaterial;
        }
    }

    void Die()
    {
        Debug.Log("Player has died!");

        // 触发死亡事件
        onDeath.Invoke();

        // 播放死亡音效
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }


        // 可以在这里添加死亡动画或游戏结束逻辑
        // gameObject.SetActive(false);
    }

    // 可选：闪烁效果协程
    /*
    IEnumerator FlashEffect()
    {
        float flashDuration = invincibleTime;
        float flashInterval = 0.1f;
        
        while(flashDuration > 0)
        {
            playerRenderer.enabled = !playerRenderer.enabled;
            flashDuration -= flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }
        
        playerRenderer.enabled = true;
    }
    */
}