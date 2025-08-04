using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class godie : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    float watertime = 1.5f;
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        watertime -= Time.deltaTime;
        if (watertime<=0)
        {
            Destroy(gameObject);
            watertime = 1.5f;
        }
    }
}
