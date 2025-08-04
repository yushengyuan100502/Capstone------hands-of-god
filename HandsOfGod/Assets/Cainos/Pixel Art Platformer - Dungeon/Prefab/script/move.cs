using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class move : MonoBehaviour
{

    public static bool right = true;

    // Start is called before the first frame update
    public float jump = 8f;
    public float speed = 10f;
    private Rigidbody rb;
    private SpriteRenderer sr;
    private CapsuleCollider cc;
    public Sprite rightmove;
    public Sprite leftmove;
    bool havego = false;
    float jumps_remaining = 2;
    public Animator anims;
    bool notrigger=false;
    bool candown = false;
    bool canup = false;
    bool doing = false;
    public static bool isit = true;
    string x = "";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sr= GetComponent<SpriteRenderer>();
        anims = GetComponent<Animator>();
        cc = GetComponent<CapsuleCollider>();
    }

    public float horizontal_friction_factor = 0.99f;

    // Update is called once per frame
    void Update()
    {
        bool on_ground = false;
        if (Physics.Raycast(transform.position, Vector3.up, 0.6f)&& (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))&&canup)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
            rb.velocity = new Vector3(rb.velocity.x, 0, 0);
            canup = false;

        }
        if (Physics.Raycast(transform.position, Vector3.down, 1.6f))
        {
            on_ground = true;
            if (rb.velocity.y <= 0)
                jumps_remaining = 2;
        }
        if (candown && Input.GetKeyDown(KeyCode.S)&& Physics.Raycast(transform.position, Vector3.down, 1f))
        {
            transform.position =new Vector3 (transform.position.x, transform.position.y - 2f, transform.position.z);
            rb.velocity = new Vector3(rb.velocity.x, 0, 0);
            candown = false;
        }



        // JUMP
        if (jumps_remaining > 0 && doing == false)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                rb.velocity = new Vector3(rb.velocity.x, jump, rb.velocity.z);
                jumps_remaining -= 1;
            }
        }

        float actual_movement_speed = speed;
        if (on_ground == false)
            actual_movement_speed = speed * 0.75f;

        // RIGHT AND LEFT
        if (Input.GetKey(KeyCode.A))
        {
            x= "left";
            isit = false;
            transform.rotation = Quaternion.Euler(0,0,0);
            if (notrigger == false)
            {
                anims.SetBool("moving",true);
            }
                rb.velocity=(new Vector3(-actual_movement_speed, rb.velocity.y, 0));
        }
        if (Input.GetKey(KeyCode.D))
        {
            x = "right";
            isit = true;
            transform.rotation = Quaternion.Euler(0, 180, 0);
            if (notrigger == false)
            {
                anims.SetBool("moving", true);
            }
                rb.velocity=(new Vector3(actual_movement_speed, rb.velocity.y, 0));
        }

        // Horizontal friction
        if (Physics.Raycast(transform.position, Vector3.down, 1.0f)==false || rb.velocity.x==0)
        {
            notrigger = true;
            anims.SetBool("moving", false);
        }
        else
        {
            notrigger = false;
            horizontal_friction_factor = 0.99f;
        }
            rb.velocity = new Vector3(rb.velocity.x * horizontal_friction_factor, rb.velocity.y, rb.velocity.z);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("lowest"))
        {
            candown = false;
        }
        else if (collision.collider.CompareTag("lowest")==false && collision.collider.transform.position.y<transform.position.y)
        {
            candown = true;
        }
        //if (collision.collider.CompareTag("can't jump") && collision.collider.transform.position.y < transform.position.y)
        //{
        //    canup = false;
        //    print("can't jump");
        //}
        //else if (collision.collider.CompareTag("can't jump")==false && collision.collider.transform.position.y < transform.position.y)
        //{
        //    canup = true;
        //}
    }

}
