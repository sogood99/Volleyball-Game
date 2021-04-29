using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    RunningForward,
    RunningBackward,
    Jumping
}

public class Athlete : MonoBehaviour
{
    // Personalized variables
    public float runSpd = 30;
    public float jumpSpd = 80;
    public Hitbox[] hitBoxes;

    // Logic Variables
    public bool leftSide;
    public bool airborne;
    public float maxGravScale = 40;
    // hitIndex is -1 if no hitBox should be out
    public int hitIndex = -1;

    // Update -> FixedUpdate
    private char runPressed;
    private bool jumpPressed = false;

    // Components
    private Rigidbody2D rb;
    public Ball ball;
    public Rigidbody2D ballRb;



    // Start is called before the first frame update
    private void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
        ballRb = ball.GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        
        Physics2D.IgnoreCollision(ball.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }



    // Update is called once per frame
    void Update()
    {
        // Check which run buttons are pressed
        if (Input.GetKey(KeyCode.D))
            // In the case of both keys pressed on the same frame, default to right
            runPressed = 'R';
        else if (Input.GetKey(KeyCode.A))
            // Go left
            runPressed = 'L';
        else
            // Go nowhere
            runPressed = 'N';


        // Check if jump is initiated
        if (Input.GetKeyDown(KeyCode.Space) && !airborne)
            jumpPressed = true;

        // Check for hit keys pressed
        if (hitIndex == -1)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                // Activate a defensive hit 
                hitIndex = 0;
                hitBoxes[hitIndex].active = true;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                // Activate an offensive hit
                hitIndex = 1;
                hitBoxes[hitIndex].active = true;
            }

            if (airborne && Input.GetKeyDown(KeyCode.Space))
            {
                // Activate an spike hit
                hitIndex = 2;
                hitBoxes[hitIndex].active = true;
            }
        }

        // If currently hitting
        if (hitIndex != -1)
        {
        }
    }

    // Put all of the rigidbody stuff in here
    private void FixedUpdate()
    {
        // Control the running
        if (runPressed == 'R')
            rb.velocity = new Vector2(runSpd, rb.velocity.y);
        else if (runPressed == 'L')
            rb.velocity = new Vector2(-runSpd * 4/5, rb.velocity.y);
        else
            rb.velocity = new Vector2(0, rb.velocity.y);


        // Upwards velocity for the jump
        if (jumpPressed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpd);
            jumpPressed = false;
        }

        // Give the player a "flea jump" by having them hang in the air
        if (rb.velocity.y <= jumpSpd * .21f && rb.velocity.y > 0)
        {
            float vScale = rb.velocity.y / (jumpSpd * .21f);
            // Gravity scales from (.01 -> 1) * max gravity
            float dynamicGravity = (.01f + (.99f * vScale)) * maxGravScale;
            rb.gravityScale = dynamicGravity;
        }
        else
            rb.gravityScale = maxGravScale;
    }



    public virtual void HitTheBall(Hitbox theBox)
    {
        // Figure out the direction the ball should be hit
        int directionX = 1;
        if (!leftSide)
            directionX = -1;

        // Calculate income variables
        Vector2 impactAngle = ballRb.velocity.normalized;
        float impactMag = ballRb.velocity.magnitude;
        float magScale = impactMag / ball.maxSpd;

        // Determine which type of hit to use
        if (theBox.type == HitType.Defense)
            DefenseHit(directionX, impactAngle, magScale);
        else if (theBox.type == HitType.Offense)
            OffenseHit(directionX, impactAngle, magScale);
        else if (theBox.type == HitType.Spike)
            SpikeHit(directionX, impactAngle, magScale);
    }

    protected virtual void DefenseHit(int directionX, Vector2 impactAngle, float magScale)
    {
        // Initialize ball return variables
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        if (runPressed == 'L')
            angle = new Vector2(-.1f, 1).normalized;
        else if (runPressed == 'N')
            angle = new Vector2(0, 1).normalized;
        else if (runPressed == 'R')
            angle = new Vector2(.2f, 1).normalized;

        // Return velocity magnitude ranges from .4 -> .9 of maxSpd
        trajectory = angle * (ball.maxSpd * .4f) * (1 + (magScale * .5f));
        ballRb.velocity = new Vector2(trajectory.x * directionX, trajectory.y);
    }

    protected virtual void OffenseHit(int directionX, Vector2 impactAngle, float magScale)
    {
        // Initialize ball return variables
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        if (runPressed == 'L')
            angle = new Vector2(1, 1.7f).normalized;
        else if (runPressed == 'N')
            angle = new Vector2(1, 1.2f).normalized;
        else if (runPressed == 'R')
            angle = new Vector2(1, .7f).normalized;
        
        // Return velocity magnitude ranges from .5 -> .8 of maxSpd
        trajectory = angle * (ball.maxSpd * .5f) * (1 + (magScale * .3f));
        ballRb.velocity = new Vector2(trajectory.x * directionX, trajectory.y);
    }

    protected virtual void SpikeHit(int directionX, Vector2 impactAngle, float magScale)
    {
        // Initialize ball return variables
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        angle = new Vector2(1, -1.4f).normalized;

        // Return velocity magnitude is .9 of maxSpd
        trajectory = angle * (ball.maxSpd * .9f);
        ballRb.velocity = new Vector2(trajectory.x * directionX, trajectory.y);
    }



    // Deal with floor collisions
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
