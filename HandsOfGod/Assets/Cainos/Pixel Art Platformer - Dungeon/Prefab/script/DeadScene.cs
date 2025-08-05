using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadScene : MonoBehaviour
{
    public bool playerLife = true;
    // Start is called before the first frame update
    void Start()
    {
        string nextSceneName = "5";
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
