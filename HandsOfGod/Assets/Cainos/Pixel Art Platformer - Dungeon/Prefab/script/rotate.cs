using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    // Start is called before the first frame update
    public bool flyright = true;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (flyright)
        {
            transform.rotation=Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation= Quaternion.Euler(0, -180, 0);
        }
    }
}
