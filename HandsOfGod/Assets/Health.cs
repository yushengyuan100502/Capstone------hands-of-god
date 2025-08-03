using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Slider healthSlider;
    public TextMeshProUGUI healthText; // 加上血量文字引用

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " 死亡");
        Destroy(gameObject);
    }
}