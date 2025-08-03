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
 
        }
        if ((Physics.Raycast(transform.position, Vector3.right, 0.6f)&& rb.velocity.x>0) || (Physics.Raycast(transform.position, Vector3.left, 0.6f)&& (rb.velocity.x<0)))
        {
            rb.velocity = Vector3.zero;
            cnt-=Time.deltaTime;
            sr.sprite = blast;
        }
        if (cnt<=0)
        {
            Destroy(gameObject);
        }
    }
}
