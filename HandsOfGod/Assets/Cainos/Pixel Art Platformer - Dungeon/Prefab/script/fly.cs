using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fly : MonoBehaviour
{
    private Rigidbody rb;
    private SpriteRenderer sr;
    public Sprite blast;
    bool die = false;
    float cnt = 0.2f;
    public float flyspeed = 0;
    public int damage = 50; // Damage dealt to enemies
    private bool hasDealtDamage = false; // Prevent multiple damage instances
    public float explosionRadius = 2f; // 爆炸范围半径

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponent<SpriteRenderer>();
    }

    public bool fly_right = true;

    void Update()
    {
        if (fly_right)
        {
            rb.velocity = new Vector3(flyspeed, 0, 0);
        }
        else
        {
            rb.velocity = new Vector3(-flyspeed, 0, 0);
            transform.rotation = Quaternion.Euler(0, 180, 90);
        }

        // 检测周围敌人
        if (!hasDealtDamage)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var hitCollider in hitColliders)
            {
                SlimeEnemy enemy = hitCollider.GetComponent<SlimeEnemy>();
                if (enemy != null)
                {
                    Explode();
                    break; // 找到一个敌人就触发爆炸
                }
            }
        }

        // 原有的射线检测（保留作为碰到墙壁时的爆炸触发）
        if (!hasDealtDamage)
        {
            RaycastHit hit;
            Vector3 rayDirection = fly_right ? Vector3.right : Vector3.left;

            if (Physics.Raycast(transform.position, rayDirection, out hit, 0.6f))
            {
                Explode();
            }
        }

        // Handle explosion countdown
        if (hasDealtDamage || sr.sprite == blast)
        {
            cnt -= Time.deltaTime;
        }
        if (cnt <= 0)
        {
            Destroy(gameObject);
        }
    }

    // 爆炸方法
    private void Explode()
    {
        if (hasDealtDamage) return; // 防止重复伤害

        // 改变外观为爆炸效果
        rb.velocity = Vector3.zero;
        sr.sprite = blast;
        hasDealtDamage = true;

        // 对范围内所有敌人造成伤害
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            SlimeEnemy enemy = hitCollider.GetComponent<SlimeEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Fireball exploded and hit enemy for " + damage + " damage!");
            }
        }
    }

    // 可选：在编辑器中显示爆炸范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}