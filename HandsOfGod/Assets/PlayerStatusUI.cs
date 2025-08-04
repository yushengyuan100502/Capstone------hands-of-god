using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("Player References")]
    public newskill playerSkills; // Reference to the player's newskill component
    public PlayerHealth playerHealth; // Reference to the player's health component
    
    [Header("Mana UI References")]
    public TextMeshProUGUI manaText; // Text component to show mana numbers
    public Slider manaBar; // Slider component for mana bar (optional)
    public Image manaBarFill; // Fill image of the slider (optional)
    
    [Header("Health UI References")]
    public TextMeshProUGUI healthText; // Text component to show health numbers
    public Slider healthBar; // Slider component for health bar (optional)
    public Image healthBarFill; // Fill image of the slider (optional)
    
    [Header("Mana UI Settings")]
    public bool showManaNumbers = true;
    public bool showManaBar = true;
    public Color fullManaColor = Color.blue;
    public Color lowManaColor = Color.red;
    public float lowManaThreshold = 0.25f; // When to show low mana color (25%)
    
    [Header("Health UI Settings")]
    public bool showHealthNumbers = true;
    public bool showHealthBar = true;
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color shieldHealthColor = Color.cyan;
    public Color invincibleHealthColor = Color.yellow;
    public float lowHealthThreshold = 0.25f; // When to show low health color (25%)
    
    void Start()
    {
        // Auto-find player components if not assigned
        if (playerSkills == null)
        {
            playerSkills = FindObjectOfType<newskill>();
            if (playerSkills == null)
            {
                Debug.LogError("PlayerStatusUI: Could not find newskill component in scene!");
            }
        }
        
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("PlayerStatusUI: Could not find PlayerHealth component in scene!");
            }
        }
        
        // Auto-find mana UI components if not assigned
        if (manaBar != null && manaBarFill == null)
            manaBarFill = manaBar.fillRect.GetComponent<Image>();
            
        // Auto-find health UI components if not assigned  
        if (healthBar != null && healthBarFill == null)
            healthBarFill = healthBar.fillRect.GetComponent<Image>();
    }
    
    void Update()
    {
        if (playerSkills != null)
            UpdateManaDisplay();
            
        if (playerHealth != null)
            UpdateHealthDisplay();
    }
    
    void UpdateManaDisplay()
    {
        float currentMana = playerSkills.GetCurrentMana();
        float maxMana = playerSkills.GetMaxMana();
        float manaPercentage = playerSkills.GetManaPercentage();
        
        // Update text display
        if (showManaNumbers && manaText != null)
        {
            manaText.text = $"Mana: {currentMana:F0}/{maxMana:F0}";
            
            // Change text color based on mana level
            if (manaPercentage <= lowManaThreshold)
            {
                manaText.color = lowManaColor;
            }
            else
            {
                manaText.color = fullManaColor;
            }
        }
        
        // Update mana bar
        if (showManaBar && manaBar != null)
        {
            manaBar.value = manaPercentage;
            
            // Change bar color based on mana level
            if (manaBarFill != null)
            {
                if (manaPercentage <= lowManaThreshold)
                {
                    manaBarFill.color = lowManaColor;
                }
                else
                {
                    manaBarFill.color = Color.Lerp(lowManaColor, fullManaColor, manaPercentage);
                }
            }
        }
    }
    
    void UpdateHealthDisplay()
    {
        int currentHealth = playerHealth.GetCurrentHealth();
        int maxHealth = playerHealth.GetMaxHealth();
        float healthPercentage = playerHealth.GetHealthPercentage();
        bool hasShield = playerHealth.HasShield();
        bool isInvincible = playerHealth.IsInvincible();
        
        // Update text display
        if (showHealthNumbers && healthText != null)
        {
            string shieldText = hasShield ? " [SHIELD]" : "";
            string invincibleText = isInvincible ? " [INV]" : "";
            healthText.text = $"Health: {currentHealth}/{maxHealth}{shieldText}{invincibleText}";
            
            // Change text color based on health level and status
            if (hasShield)
            {
                healthText.color = shieldHealthColor;
            }
            else if (isInvincible)
            {
                healthText.color = invincibleHealthColor;
            }
            else if (healthPercentage <= lowHealthThreshold)
            {
                healthText.color = lowHealthColor;
            }
            else
            {
                healthText.color = fullHealthColor;
            }
        }
        
        // Update health bar
        if (showHealthBar && healthBar != null)
        {
            healthBar.value = healthPercentage;
            
            // Change bar color based on health level and status
            if (healthBarFill != null)
            {
                if (hasShield)
                {
                    healthBarFill.color = shieldHealthColor;
                }
                else if (isInvincible)
                {
                    healthBarFill.color = invincibleHealthColor;
                }
                else if (healthPercentage <= lowHealthThreshold)
                {
                    healthBarFill.color = lowHealthColor;
                }
                else
                {
                    healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);
                }
            }
        }
    }
    
    // Call this method to flash the UI when player tries to use abilities without enough mana
    public void FlashLowMana()
    {
        StartCoroutine(FlashLowManaEffect());
    }
    
    // Call this method to flash the health UI when player takes damage
    public void FlashDamage()
    {
        StartCoroutine(FlashDamageEffect());
    }
    
    private IEnumerator FlashLowManaEffect()
    {
        Color originalTextColor = manaText != null ? manaText.color : Color.white;
        Color originalBarColor = manaBarFill != null ? manaBarFill.color : Color.white;
        
        // Flash red for a short time
        for (int i = 0; i < 3; i++)
        {
            if (manaText != null)
                manaText.color = Color.red;
            if (manaBarFill != null)
                manaBarFill.color = Color.red;
                
            yield return new WaitForSeconds(0.1f);
            
            if (manaText != null)
                manaText.color = originalTextColor;
            if (manaBarFill != null)
                manaBarFill.color = originalBarColor;
                
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator FlashDamageEffect()
    {
        Color originalTextColor = healthText != null ? healthText.color : Color.white;
        Color originalBarColor = healthBarFill != null ? healthBarFill.color : Color.white;
        
        // Flash red for a short time
        for (int i = 0; i < 3; i++)
        {
            if (healthText != null)
                healthText.color = Color.red;
            if (healthBarFill != null)
                healthBarFill.color = Color.red;
                
            yield return new WaitForSeconds(0.1f);
            
            if (healthText != null)
                healthText.color = originalTextColor;
            if (healthBarFill != null)
                healthBarFill.color = originalBarColor;
                
            yield return new WaitForSeconds(0.1f);
        }
    }
}