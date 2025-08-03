using UnityEngine;
using UnityEngine.Events; // �����¼�ϵͳ

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings/Ѫ������")]
    [Tooltip("Max health/���Ѫ��")]
    public int maxHealth = 100; // [�ɵ���] ��ҳ�ʼѪ��

    [Tooltip("Current health/��ǰѪ��")]
    public int currentHealth; // ��ǰѪ��������Start�г�ʼ����

    [Tooltip("Invincible time after hit/�����޵�ʱ��")]
    public float invincibleTime = 1f; // [�ɵ���] ���˺���޵�ʱ��(��)

    [Header("Effects/Ч��")]
    [Tooltip("Damage sound effect/������Ч")]
    public AudioClip damageSound;

    [Tooltip("Death sound effect/������Ч")]
    public AudioClip deathSound;

    [Tooltip("Damage visual effect/�����Ӿ�Ч��")]
    public Material damageMaterial; // ����ʱ��ʱ�滻�Ĳ���
    private Material originalMaterial; // ԭʼ����

    [Header("Events/�¼�")]
    public UnityEvent onDamageTaken; // ����ʱ�������¼�
    public UnityEvent onDeath; // ����ʱ�������¼�

    // ˽�б���
    private AudioSource audioSource;
    private Renderer playerRenderer;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    void Start()
    {
        // ��ʼ�����
        audioSource = GetComponent<AudioSource>();
        playerRenderer = GetComponentInChildren<Renderer>();

        // �洢ԭʼ����
        if (playerRenderer != null)
        {
            originalMaterial = playerRenderer.material;
        }

        // ��ʼ��Ѫ��
        currentHealth = maxHealth;
    }

    void Update()
    {
        // �޵�ʱ���ʱ��
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0)
            {
                EndInvincibility();
            }
        }
    }

    // �ܵ��˺��Ĺ����������ᱻZombieAI���ã�
    public void TakeDamage(int damage)
    {
        // ��������޵�״̬������˺�
        if (isInvincible || currentHealth <= 0) return;

        // �۳�Ѫ��
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage! Current health: {currentHealth}");

        // ���������¼�
        onDamageTaken.Invoke();

        // ����������Ч
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Ӧ�������Ӿ�Ч��
        if (damageMaterial != null && playerRenderer != null)
        {
            playerRenderer.material = damageMaterial;
        }

        // ��ʼ�޵�ʱ��
        StartInvincibility();

        // ����Ƿ�����
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ����Ч�����ᱻZombieAI���ã�
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

        // ���������������˸Ч��
        // StartCoroutine(FlashEffect());
    }

    void EndInvincibility()
    {
        isInvincible = false;

        // �ָ�ԭʼ����
        if (playerRenderer != null && originalMaterial != null)
        {
            playerRenderer.material = originalMaterial;
        }
    }

    void Die()
    {
        Debug.Log("Player has died!");

        // ���������¼�
        onDeath.Invoke();

        // ����������Ч
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }


        // �������������������������Ϸ�����߼�
        // gameObject.SetActive(false);
    }

    // ��ѡ����˸Ч��Э��
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