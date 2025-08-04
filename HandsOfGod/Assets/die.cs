using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class die : MonoBehaviour
{
    // Start is called before the first frame update
    public float dieing = 0.5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        dieing-=Time.deltaTime;
        if (dieing<=0)
        {
            Destroy(gameObject);
        }
    }
}
