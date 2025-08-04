using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class newskill : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject shieldPrefab;  // 护盾的预制体
    private GameObject currentShield; // 当前生成的护盾
    public float shieldDuration = 3f; // 护盾持续时间
    private bool isShieldActive = false;
    public GameObject fireball;
    public move mooo;
    public GameObject firepillar;
    public float heldtime = 0.3f;
    float time = 0f;
    bool isfire = false;
    float cd = 0.1f;
    Rigidbody rb;

    private GameObject fire;
    
    [Header("Mana System")]
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegenRate = 10f; // Mana per second
    public float fireballManaCost = 20f;
    public float firePillarManaCost = 40f;
    public float shieldManaCost = 30f;
    
    [Header("UI References")]
    public PlayerStatusUI statusUI; // Reference to the player status UI (optional)
    
    // Update is called once per frame
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mooo = GetComponent<move>();
        
        // Initialize mana system
        currentMana = maxMana;
        
        // Auto-find PlayerStatusUI if not assigned
        if (statusUI == null)
        {
            statusUI = FindObjectOfType<PlayerStatusUI>();
        }
    }
    IEnumerator RemoveShieldAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentShield != null)
        {
         Destroy(currentShield);
        }
        isShieldActive = false;

        // 取消护盾免疫状态
        GetComponent<PlayerHealth>().hasShield = false;
    }
    void Update()
    {
        // Mana regeneration
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
        }
        
        // Shield ability with mana check
        if (Input.GetMouseButtonDown(1) && !isShieldActive && HasEnoughMana(shieldManaCost))
        {
            UseMana(shieldManaCost);
            
            currentShield = Instantiate(shieldPrefab, transform);

            float offsetY = 1f;
            currentShield.transform.localPosition = new Vector3(0, offsetY, 0f);

            if (!move.isit)
            {   
                Vector3 localScale = currentShield.transform.localScale;
                localScale.x = -Mathf.Abs(localScale.x);
                currentShield.transform.localScale = localScale;
            }

            // 设置护盾状态
            isShieldActive = true;
            GetComponent<PlayerHealth>().hasShield = true;

            StartCoroutine(RemoveShieldAfterDelay(shieldDuration));
            
            Debug.Log("Shield activated! Mana remaining: " + currentMana);
        }
        else if (Input.GetMouseButtonDown(1) && !isShieldActive && !HasEnoughMana(shieldManaCost))
        {
            Debug.Log("Not enough mana for shield! Need: " + shieldManaCost + ", Have: " + currentMana);
            if (statusUI != null)
                statusUI.FlashLowMana();
        }
        // Handle input when not enough mana to start charging
        if ((Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0)) && !HasEnoughMana(fireballManaCost))
        {
            Debug.Log("Not enough mana to cast fireball! Need: " + fireballManaCost + ", Have: " + currentMana);
            if (statusUI != null)
                statusUI.FlashLowMana();
        }
        
        // Check for both Q key and left mouse button for charging (with mana check)
        if ((Input.GetKey(KeyCode.Q) || Input.GetMouseButton(0)) && HasEnoughMana(fireballManaCost))
        {
            time += Time.deltaTime;
            if (time > heldtime && HasEnoughMana(firePillarManaCost))
            {
                if (isfire == false)
                {
                    fire = Instantiate(firepillar);
                }
                fire.GetComponent<rotate>().flyright = move.isit;
                isfire = true;
                if (move.isit == false)
                {
                    fire.transform.position = new Vector3(transform.position.x - 3.3f, transform.position.y, transform.position.z);
                }
                else
                {
                    fire.transform.position = new Vector3(transform.position.x + 3.3f, transform.position.y, transform.position.z);
                }
            }
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            rb.useGravity = false;

        }
        // Check for both Q key release and left mouse button release
        if (Input.GetKeyUp(KeyCode.Q) || Input.GetMouseButtonUp(0))
        {
            if (time > 0) // Only process if we were actually charging
            {
                if (time < heldtime && HasEnoughMana(fireballManaCost))
                {
                    // Fireball
                    UseMana(fireballManaCost);
                    
                    GameObject new_fireball = Instantiate(fireball);
                    new_fireball.transform.rotation = Quaternion.Euler(transform.rotation.x,-transform.rotation.y,90f);
                    new_fireball.GetComponent<fly>().fly_right = move.isit;
                    if (move.isit == false)
                    {
                        new_fireball.transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y, transform.position.z);
                    }
                    else
                    {
                        new_fireball.transform.position = new Vector3(transform.position.x + 0.8f, transform.position.y, transform.position.z);
                    }
                    
                    Debug.Log("Fireball cast! Mana remaining: " + currentMana);
                }
                else if (time >= heldtime && HasEnoughMana(firePillarManaCost))
                {
                    // Fire pillar
                    UseMana(firePillarManaCost);
                    DestroyImmediate(fire);
                    
                    Debug.Log("Fire pillar cast! Mana remaining: " + currentMana);
                }
                else if (time >= heldtime && !HasEnoughMana(firePillarManaCost))
                {
                    // Not enough mana for fire pillar, clean up
                    if (fire != null)
                        DestroyImmediate(fire);
                    Debug.Log("Not enough mana for fire pillar! Need: " + firePillarManaCost + ", Have: " + currentMana);
                    if (statusUI != null)
                        statusUI.FlashLowMana();
                }
                else if (time < heldtime && !HasEnoughMana(fireballManaCost))
                {
                    Debug.Log("Not enough mana for fireball! Need: " + fireballManaCost + ", Have: " + currentMana);
                    if (statusUI != null)
                        statusUI.FlashLowMana();
                }
                
                time = 0f;
                isfire = false;
                rb.useGravity = true;
            }
        }
    }
    
    // Mana utility methods
    private bool HasEnoughMana(float manaCost)
    {
        return currentMana >= manaCost;
    }
    
    private void UseMana(float manaCost)
    {
        currentMana -= manaCost;
        currentMana = Mathf.Max(currentMana, 0f);
    }
    
    // Public method to get current mana (for UI or other systems)
    public float GetCurrentMana()
    {
        return currentMana;
    }
    
    public float GetMaxMana()
    {
        return maxMana;
    }
    
    public float GetManaPercentage()
    {
        return currentMana / maxMana;
    }
}
