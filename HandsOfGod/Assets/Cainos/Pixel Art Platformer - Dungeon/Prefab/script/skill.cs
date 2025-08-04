using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skill : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject fireball;
    public move mooo;
    public GameObject firepillar;
    public GameObject water;
    public float heldtime = 0.3f;
    float time = 0f;
    bool isfire = false;
    float cd = 0.1f;
    Rigidbody rb;
    bool iswater = false;
    float watertime = 1.5f;
    public bool shalldie = false;
    public Sprite magic;
    public Sprite walk;
    SpriteRenderer sr;
    Animator ani;

    private GameObject fire;
    
    // Update is called once per frame
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mooo = GetComponent<move>();
        sr= GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();
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
            transform.rotation = new Quaternion (0,transform.rotation.y , 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            rb.useGravity = false;
            
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (time<heldtime)
            {
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
            }
            else
            {
                DestroyImmediate(fire);
            }
            time = 0f;
            isfire = false;
            rb.useGravity = true;
        }
        if (Input.GetKeyDown(KeyCode.F)&&iswater==false && Physics.Raycast(transform.position, Vector3.down))
        {
            shalldie = false;
            GameObject watershield = Instantiate(water);
            watershield.transform.position = transform.position;
            iswater = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        if (iswater)
        {
            watertime-= Time.deltaTime;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            sr.sprite = magic;
            ani.enabled = false;
        }
        if (watertime<=0)
        {
            iswater = false;
            shalldie = true;
            watertime = 1.5f;
            sr.sprite = walk;
            ani.enabled = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        }
    }
}
