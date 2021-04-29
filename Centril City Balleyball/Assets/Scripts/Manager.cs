using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // If true, this will turn on hitbox sprites
    public bool debugMode;

    // Gather a list of all athletes
    public GameObject[] allAthletes;

    // Get the ball
    public GameObject ball;



    // Start is called before the first frame update
    void Start()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");

        allAthletes = GameObject.FindGameObjectsWithTag("Athlete");
        // Get all athletes to ignore collisions with each other and the ball
        for (int i = 0; i < allAthletes.Length; i++)
        {
            Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());

            if (i != 0)
                Physics2D.IgnoreCollision(allAthletes[0].GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());
        }

        debugMode = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle debug mode with P
        if (Input.GetKeyDown(KeyCode.P))
            debugMode = !debugMode;

        // I guess this always needs to be updated
        allAthletes = GameObject.FindGameObjectsWithTag("Athlete");
    }
}
