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
    float cdfire = 0.2f;
    float magictime = 0.1f;
    Rigidbody rb;
    bool iswater = false;
    float watertime = 1.5f;
    public bool shalldie = false;
    public Sprite magic;
    public bool magicing = false;
    public Sprite walk;
    SpriteRenderer sr;
    Animator ani;
    PlayerHealth ph;

    private GameObject fire;
    
    // Update is called once per frame
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mooo = GetComponent<move>();
        sr= GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();
        ph = GetComponent<PlayerHealth>();
    }
    void Update()
    {
        cdfire-= Time.deltaTime;
        if (magicing == true && iswater == false && magictime >= 0)
        {
            magictime -= Time.deltaTime;
            ani.enabled = false;
            sr.sprite = magic;
            mooo.enabled = false;

        }
        else if (magicing==false && magictime<=0)
        {
            ani.enabled = true;
            magictime = 0.1f;
            mooo.enabled = true ;
        }
        if (Input.GetKeyDown(KeyCode.Q) && magicing == false && cdfire <= 0)
        {
            cdfire = 0.2f;
            magicing = true;
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
            magicing = false;
        }
        if (Input.GetKeyDown(KeyCode.F)&&iswater==false && Physics.Raycast(transform.position, Vector3.down) && magicing==false)
        {
            shalldie = false;
            magicing = true;
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
            ph.watering= true;
            ani.enabled = false;

        }
        if (watertime<=0)
        {
            iswater = false;
            shalldie = true;
            watertime = 1.5f;
            sr.sprite = walk;
            ani.enabled = true;
            ph.watering = false;
            magicing=false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        }
    }
}
