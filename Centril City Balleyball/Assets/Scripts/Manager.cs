using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
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

            for (int n = 0; n < allAthletes.Length; n++)
                if (n != i)
                    Physics2D.IgnoreCollision(allAthletes[n].GetComponent<Collider2D>(), allAthletes[i].GetComponent<Collider2D>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        // I guess this always needs to be updated
        allAthletes = GameObject.FindGameObjectsWithTag("Athlete");
    }
}
