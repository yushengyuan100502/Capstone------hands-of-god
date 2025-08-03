using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skill : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject fireball;
    public GameObject firepillar;
    public float heldtime = 0.3f;
    float time = 0f;
    bool isfire = false;
    float cd = 0.1f;
    Rigidbody rb;

    private GameObject fire;
    
    // Update is called once per frame
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            time += Time.deltaTime;
            if (time > heldtime)
            {
                if (isfire==false)
                {
                    fire = Instantiate(firepillar);
                    fire.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                }
                fire.GetComponent<rotate>().flyright = move.right;
                isfire = true;
                if (move.right == false)
                {
                    fire.transform.position = new Vector3(transform.position.x - 3.3f, transform.position.y, transform.position.z);
                }
                else
                {
                    fire.transform.position = new Vector3(transform.position.x + 3.3f, transform.position.y, transform.position.z);
                }
            }
            transform.rotation = new Quaternion (0,transform.rotation.y , 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            rb.useGravity = false;
            
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (time<heldtime)
            {
                GameObject new_fireball = Instantiate(fireball);
                new_fireball.transform.rotation = transform.rotation;
                new_fireball.GetComponent<fly>().fly_right = move.right;
                if (move.right == false)
                {
                    new_fireball.transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y, transform.position.z);
                }
                else
                {
                    new_fireball.transform.position = new Vector3(transform.position.x + 0.8f, transform.position.y, transform.position.z);
                }
            }
            else
            {
                DestroyImmediate(fire);
            }
            time = 0f;
            isfire = false;
            rb.useGravity = true;
        }
    }
}
