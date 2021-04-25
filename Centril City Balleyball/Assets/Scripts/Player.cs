using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;

    public float runSpd = 15;
    public const string RIGHT = "right";
    public const string LEFT = "left";
    public string runPressed;
    public string prevRun;

    public float jumpSpd = 25;
    public bool jumpPressed = false;

    public bool airborne;

    // Start is called before the first frame update
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
            // In case both keys are pressed
            if (Input.GetKey(KeyCode.A))
            {
                // Go with the most recent presss
                if (prevRun == RIGHT)
                    runPressed = LEFT;
                // In the case of both keys pressed on the same frame, default to right
                else
                    runPressed = RIGHT;
            }
            // Otherwise, just go right
            else
                runPressed = RIGHT;
        }
        else if (Input.GetKey(KeyCode.A))
            // Go left
            runPressed = LEFT;
        else
            // Go nowhere
            runPressed = null;

        // Update previous run key pressed
        prevRun = runPressed;


        // Check if jump is initiated
        if (Input.GetKeyDown(KeyCode.Space) && !airborne)
            jumpPressed = true;
    }

    // Put all of the rigidbody stuff in here
    private void FixedUpdate()
    {
        // This is not an accurate representation of physics
        // However, it feels better to control
        if (runPressed == RIGHT)
            rb.velocity = new Vector2(runSpd, rb.velocity.y);
        else if (runPressed == LEFT)
            rb.velocity = new Vector2(-runSpd, rb.velocity.y);
        else
            rb.velocity = new Vector2(0, rb.velocity.y);

        // Upwards velocity for the jump
        if (jumpPressed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpd);
            jumpPressed = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor")
            airborne = false;
        if (collision.gameObject.name == "Wall")
            rb.velocity = new Vector2(0, rb.velocity.y);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor")
            airborne = true;
    }
}
