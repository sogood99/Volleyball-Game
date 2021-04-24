using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;

    public float runSpd = 100;
    public const string RIGHT = "right";
    public const string LEFT = "left";
    public string runPressed;

    public float jumpSpd = 20;
    public bool jumpPressed = false;

    public bool airborne;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check which run buttons are pressed
        if (Input.GetKey(KeyCode.D))
        {
            runPressed = RIGHT;

            // Null if both keys are pressed
            if (Input.GetKey(KeyCode.A))
                runPressed = null;
        }
        else if (Input.GetKey(KeyCode.A))
            runPressed = LEFT;
        else
            runPressed = null;

        // Check if jump is initiated
        if (Input.GetKeyDown(KeyCode.Space) && !airborne)
            jumpPressed = true;

        Debug.Log("hello");
    }

    private void FixedUpdate()
    {
        if (runPressed == RIGHT)
        {
            rb.AddForce(new Vector2(runSpd, 0), ForceMode2D.Impulse);
        }
        else if (runPressed == LEFT)
        {
            rb.AddForce(new Vector2(-runSpd, 0), ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        

        if (jumpPressed)
            rb.AddForce(new Vector2(0, jumpSpd), ForceMode2D.Impulse);
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
