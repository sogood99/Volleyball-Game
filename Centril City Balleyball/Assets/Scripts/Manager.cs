using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // If true, this will turn on hitbox sprites
    public bool debugMode;



    // Start is called before the first frame update
    void Start()
    {
        debugMode = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle debug mode with P
        if (Input.GetKeyDown(KeyCode.P))
        {
            debugMode = !debugMode;
            Debug.Log(debugMode);
        }
    }
}
