using UnityEngine;
using System.Collections.Generic;

public class FireDamage : MonoBehaviour
{
    public int damageAmount = 5;
    public float damageInterval = 1f;

    private Dictionary<PlayerHealth, float> damageTimers = new Dictionary<PlayerHealth, float>();
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(15);
                Debug.Log("Slime touched player for " + 15 + " contact damage!");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null && !player.hasShield)
            {
                if (!damageTimers.ContainsKey(player))
                {
                    damageTimers[player] = 0f;
                }

                damageTimers[player] += Time.deltaTime;

                if (damageTimers[player] >= damageInterval)
                {
                    player.DirectFireDamage(damageAmount);  // 新函数，不触发isInvincible
                    damageTimers[player] = 0f;
                }
            }
        }
    }

}