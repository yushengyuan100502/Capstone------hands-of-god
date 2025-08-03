using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader2 : MonoBehaviour

{
    public bool door = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame

    private void Update()
    {
        if (door && Input.GetKeyDown(KeyCode.V))
        {
            string nextSceneName = "3";
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        print("Trigger");
        if (other.CompareTag("Door"))
        {
            door = true;
            print("Enter");
        }
        else
        {
            door = false;
        }

    }
}