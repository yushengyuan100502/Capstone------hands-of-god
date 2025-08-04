using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public bool isInvincible = false; // 无敌状态（受击后短暂无敌）
    public float invincibilityTime = 2.5f;
    private float invincibilityTimer = 0f;
    public bool watering=false;
    public SpriteRenderer spriteRenderer; // 用于受击闪烁效果

    void Start()
    {
        currentHealth = maxHealth;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        // 无敌状态计时
        if (isInvincible)
        {
            invincibilityTimer += Time.deltaTime;
            if (invincibilityTimer >= invincibilityTime)
            {
                isInvincible = false;
                invincibilityTimer = 0f;
                spriteRenderer.color = Color.white; // 恢复颜色
            }
            else
            {
                // 受击闪烁效果
                spriteRenderer.color = new Color(1, 1, 1, Mathf.PingPong(Time.time * 10f, 1f));
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible | watering) return; // 无敌状态下不受伤害

        currentHealth -= damage;
        Debug.Log($"玩家受到 {damage} 点伤害，剩余生命值: {currentHealth}");

        // 触发无敌状态
        isInvincible = true;
        invincibilityTimer = 0f;

        // 死亡检测
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("玩家死亡");
        // 这里可以添加死亡动画、游戏结束逻辑等
        Destroy(gameObject);
    }
}