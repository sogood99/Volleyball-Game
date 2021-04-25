using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody2D rb;
    public float maxSpd = 1;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Put all of the rigidbody stuff in here
    private void FixedUpdate()
    {
        // Clamp the velocity magnitude
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpd);
    }
}
