using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    private Vector3 position;
    private float xVelocity;

    public float speed = .025f;
    public float jump = 20;

    bool airborne;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Update position
        position = transform.position;

        // Reset velocity before checking presses
        xVelocity = 0;

        // change velocity if direction key is pressed
        if (Input.GetKey(KeyCode.A))
            xVelocity = -speed * .75f;
        if (Input.GetKey(KeyCode.D))
            xVelocity = speed;

        // Add velocity to position
        position.x += xVelocity;

        // Move the vehicle to its new position
        transform.position = position;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (Input.GetKeyDown(KeyCode.Space) && !airborne)
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + jump, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor")
            airborne = false;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor")
            airborne = true;
    }
}
