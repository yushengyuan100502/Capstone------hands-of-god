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
    
    // Update is called once per frame
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mooo = GetComponent<move>();
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
    void Update()
    {
            if (Input.GetMouseButtonDown(1) && !isShieldActive)
    {
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
    }
        if (Input.GetKey(KeyCode.Q))
        {
            time += Time.deltaTime;
            if (time > heldtime)
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
    }
}
