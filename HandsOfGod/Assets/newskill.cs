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

    [Header("Knockback Skill")]
    public float knockbackForce = 15f; // 击飞力度
    public float knockbackRadius = 3f; // 击飞范围
    public float knockbackManaCost = 30f; // 击飞技能魔力消耗
    private float knockbackCooldown = 1f; // 冷却时间
    private float lastKnockbackTime = -1f; // 上次使用击飞技能的时间
    public float cameraShakeDuration = 0.2f; // 镜头晃动持续时间
    public float cameraShakeMagnitude = 0.1f; // 镜头晃动强度

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

    // 镜头晃动协程
    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }

    // 击飞技能方法
    private void PerformKnockback()
    {
        // 屏幕震动
        StartCoroutine(ShakeCamera(cameraShakeDuration, cameraShakeMagnitude));

        // 查找范围内的敌人
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, knockbackRadius);
        foreach (var hitCollider in hitColliders)
        {
            // 检查是否是敌人
            if (hitCollider.CompareTag("ememy"))
            {
                Rigidbody enemyRb = hitCollider.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    // 计算击飞方向（从玩家指向敌人）
                    Vector3 direction = (hitCollider.transform.position - transform.position).normalized;

                    // 添加击飞力
                    enemyRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                }
            }
        }
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
        if (Input.GetKeyDown(KeyCode.E) && !isShieldActive && HasEnoughMana(shieldManaCost))
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
        else if (Input.GetKeyDown(KeyCode.E) && !isShieldActive && !HasEnoughMana(shieldManaCost))
        {
            Debug.Log("Not enough mana for shield! Need: " + shieldManaCost + ", Have: " + currentMana);
            if (statusUI != null)
                statusUI.FlashLowMana();
        }

        // 击飞技能
        if (Input.GetKeyDown(KeyCode.F) && Time.time - lastKnockbackTime > knockbackCooldown)
        {
            if (HasEnoughMana(knockbackManaCost))
            {
                UseMana(knockbackManaCost);
                PerformKnockback();
                lastKnockbackTime = Time.time;
                Debug.Log("Knockback performed! Mana remaining: " + currentMana);
            }
            else
            {
                Debug.Log("Not enough mana for knockback! Need: " + knockbackManaCost + ", Have: " + currentMana);
                if (statusUI != null)
                    statusUI.FlashLowMana();
            }
        }

        // Check if player is moving
        bool isPlayerMoving = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);

        // Handle input when not enough mana to start charging
        if (Input.GetKeyDown(KeyCode.Q) && !HasEnoughMana(fireballManaCost))
        {
            Debug.Log("Not enough mana to cast fireball! Need: " + fireballManaCost + ", Have: " + currentMana);
            if (statusUI != null)
                statusUI.FlashLowMana();
        }

        // Handle input when player is moving (prevent fireball casting while moving)
        if (Input.GetKeyDown(KeyCode.Q) && isPlayerMoving)
        {
            Debug.Log("Cannot cast fireball while moving! Stand still to cast.");
        }

        // Check for both Q key and left mouse button for charging (with mana check)
        // Only allow fireball casting when player is standing still (not pressing A or D)
        if (Input.GetKey(KeyCode.Q) && HasEnoughMana(fireballManaCost) && !isPlayerMoving)
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
        // Reset charging if player starts moving while charging
        else if (isPlayerMoving && time > 0)
        {
            time = 0f;
            isfire = false;
            rb.useGravity = true;
            Debug.Log("Fireball charging cancelled - player started moving!");
        }
        // Check for both Q key release and left mouse button release
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (time > 0) // Only process if we were actually charging
            {
                if (time < heldtime && HasEnoughMana(fireballManaCost))
                {
                    // Fireball
                    UseMana(fireballManaCost);

                    GameObject new_fireball = Instantiate(fireball);
                    new_fireball.transform.rotation = Quaternion.Euler(transform.rotation.x, -transform.rotation.y, 90f);
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

    // 可选：用于调试显示击飞范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, knockbackRadius);
    }
}