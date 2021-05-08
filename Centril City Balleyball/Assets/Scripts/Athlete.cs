using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Running,
    Reversing,
    Jumping
}

public class Athlete : MonoBehaviour
{
    // Personalized variables
    public float runSpd;
    public float jumpSpd;
    public Hitbox[] hitboxes;
    public int activeHitbox = -1;

    // Logic Variables
    public bool leftSide;
    public bool airborne;
    public bool hitting;
    public float maxGravScale = 40;
    public bool onSomeone = false;
    public bool underSomeone = false;

    // Update -> FixedUpdate
    private char runPressed;
    private bool jumpPressed = false;

    // Components
    private Rigidbody2D rb;
    public BoxCollider2D mountTrigger;
    public Ball ball;
    public Rigidbody2D ballRb;
    public Manager worldManager;
    private Transform hurtbox;
    public Animator legAnimator;
    public Animator mainAnimator;



    // Start is called before the first frame update
    private void Start()
    {
        worldManager = GameObject.FindGameObjectWithTag("World").GetComponent<Manager>();
        rb = GetComponent<Rigidbody2D>();
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
        ballRb = ball.GetComponent<Rigidbody2D>();
    }



    // Update is called once per frame
    void Update()
    {
        // Check if standing on an athlete
        if (onSomeone || underSomeone)
        {
            // You can't move off
            runPressed = 'N';
        }
        else
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
        }


        // Check if jump is initiated
        if (Input.GetKeyDown(KeyCode.Space) && !airborne)
            jumpPressed = true;

        // Temporarily lock control to Player
        if (name == "Player")
        {
            // Check for hit keys pressed
            if (!hitting)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    // Activate a defense hit 
                    hitboxes[0].active = true;
                    activeHitbox = 0;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    // Activate an offense hit
                    hitboxes[1].active = true;
                    activeHitbox = 1;
                }
                if (airborne && Input.GetKeyDown(KeyCode.Space))
                {
                    // Activate an spike hit
                    hitboxes[2].active = true;
                    activeHitbox = 2;
                }
            }
        }

        UpdateAnimator();
    }

    // Put all of the rigidbody stuff in here
    private void FixedUpdate()
    {
        // Temporarily lock control to Player
        if (name == "Player")
        {
            // Control the running
            if (runPressed == 'R')
            {
                rb.velocity = new Vector2(runSpd, rb.velocity.y);
            }
            else if (runPressed == 'L')
                rb.velocity = new Vector2(-runSpd * 4 / 5, rb.velocity.y);
            else
                rb.velocity = new Vector2(0, rb.velocity.y);

            // Move slower in the air
            if (airborne)
                rb.velocity = new Vector2(rb.velocity.x * .9f, rb.velocity.y);


            // Upwards velocity for the jump
            if (jumpPressed)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpSpd);
                jumpPressed = false;
            }

            // Give the athlete a "flea jump" by having them hang in the air
            if (rb.velocity.y <= jumpSpd * .21f && rb.velocity.y > 0)
            {
                float vScale = rb.velocity.y / (jumpSpd * .21f);
                // Gravity scales from (.01 -> 1) * max gravity
                float dynamicGravity = (.01f + (.99f * vScale)) * maxGravScale;
                rb.gravityScale = dynamicGravity;
            }
            else
                rb.gravityScale = maxGravScale;

            // Check if about to land on another athlete
            // Eventually have it only check on same team
            foreach (GameObject otherGuy in worldManager.allAthletes)
            {
                if (otherGuy != gameObject)
                {
                    // Check if other athlete is grounded, and this athlete is falling
                    if (!otherGuy.GetComponent<Athlete>().airborne && rb.velocity.y < 0)
                    {
                        Bounds theseBounds = GetComponent<Collider2D>().bounds;
                        Bounds thoseBounds = otherGuy.GetComponent<Collider2D>().bounds;
                        // Check if this athlete will pass through the other athlete's collider vertically
                        if (theseBounds.min.y + rb.velocity.y <= thoseBounds.max.y)
                        {
                            float inverseSlope = rb.velocity.x / rb.velocity.y;
                            float distY = theseBounds.min.y + rb.velocity.y - thoseBounds.max.y;
                            float distX = inverseSlope * distY;
                            // Check if this athlete will land on the other athlete's collider horizontally
                            // The horizontal area is restricted for a tighter landing requirement
                            if (Mathf.Abs(theseBounds.center.x + distX - thoseBounds.center.x) <= thoseBounds.extents.x)
                            {
                                // If all checks are passed, allow collisions
                                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherGuy.GetComponent<Collider2D>(), false);
                            }
                        }

                    }
                }
            }
        }
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
        if (collision.gameObject.tag == "Floor")
            airborne = false;
        else if (collision.gameObject.tag == "Athlete")
        {
            // If above the other athlete
            if (transform.position.y > collision.gameObject.transform.position.y)
            {
                airborne = false;
                onSomeone = true;

                // Move this athlete to the center of the other
                transform.position = new Vector2(collision.gameObject.transform.position.x, transform.position.y);
            }
            // If below the other athlete
            else
            {
                underSomeone = true;

                if (collision.gameObject.layer < gameObject.layer)
                {
                    // If the athlete on top has a lower layer, swap the two
                    int myLayer = gameObject.layer;
                    gameObject.layer = collision.gameObject.layer;
                    collision.gameObject.layer = myLayer;
                }
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
            airborne = true;
        else if (collision.gameObject.tag == "Athlete")
        {
            // If above the other athlete
            if (transform.position.y > collision.gameObject.transform.position.y)
            {
                airborne = true;
                onSomeone = false;
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>());
            }
            // If below the other athlete
            else
            {
                underSomeone = false;
            }
        }
    }

    private void UpdateAnimator()
    {
        legAnimator.SetFloat("xVelocity", rb.velocity.x);
        legAnimator.SetBool("airborne", airborne);
        legAnimator.SetBool("mounted", underSomeone);

        mainAnimator.SetBool("airborne", airborne);
        mainAnimator.SetBool("mounted", underSomeone);
        mainAnimator.SetInteger("hitType", activeHitbox);
    }
}
