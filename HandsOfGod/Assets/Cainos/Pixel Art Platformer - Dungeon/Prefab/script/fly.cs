using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fly : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    private SpriteRenderer sr;
    public Sprite blast;
    bool die = false;
    float cnt = 0.2f;
    public float flyspeed = 0;
    public int damage = 50; // Damage dealt to enemies
    private bool hasDealtDamage = false; // Prevent multiple damage instances
    
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        sr=GetComponent<SpriteRenderer>();
    }

    public bool fly_right = true;

    // Update is called once per frame
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
        // Only check for collisions if we haven't dealt damage yet
        if (!hasDealtDamage)
        {
            RaycastHit hit;
            Vector3 rayDirection = fly_right ? Vector3.right : Vector3.left;
            
            if (Physics.Raycast(transform.position, rayDirection, out hit, 0.6f))
            {
                // Check if we hit an enemy
                SlimeEnemy enemy = hit.collider.GetComponent<SlimeEnemy>();
                if (enemy != null && !hasDealtDamage)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log("Fireball hit enemy for " + damage + " damage!");
                    hasDealtDamage = true; // Mark that we've dealt damage
                }
                
                // Hit something (wall or enemy), start exploding
                rb.velocity = Vector3.zero;
                sr.sprite = blast;
                hasDealtDamage = true; // Prevent further damage
            }
        }
        
        // Handle explosion countdown
        if (hasDealtDamage || sr.sprite == blast)
        {
            cnt -= Time.deltaTime;
        }
        if (cnt<=0)
        {
            Destroy(gameObject);
        }
    }
}
