using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MobilityState
{
    Free,
    Mounting,
    Mounted,
    Stunned,
    Taunting
}

public enum HitState
{
    None,
    Offensive,
    Defensive,
    SpikeWind,
    SpikeHit
}

public class Athlete : MonoBehaviour
{
    // ----------------------------------
    // - - - - - INITIALIZATION - - - - -
    // ----------------------------------

    // Control Fields
    private Dictionary<string, KeyCode> controls;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
    public KeyCode offenseKey;
    public KeyCode defenseKey;

    // Hit Fields
    private int hitTimer = 0;
    private int offenseDuration = 60;
    private int defenseDuration = 60;
    private int spikeWindDuration = 5;
    private int spikeHitDuration = 20;

    // Logic fields
    public float runSpd;
    public float jumpSpd;
    public bool leftSide;
    public float maxGravScale;

    // State fields
    public MobilityState moveState;
    public HitState hitState;
    public bool airborne;
    public bool mountJump = false;
    public bool serving;

    // Communication fields
    private char runPressed;
    private bool jumpPressed = false;

    // Hurtbox Fields
    private Rigidbody2D rb;
    public BoxCollider2D hurtbox;
    public Vector2 footPoint;
    public float footPointOffset;
    public CapsuleCollider2D mountTrigger;

    // Component fields
    public Ball ball;
    public Rigidbody2D ballRb;
    public Manager worldManager;
    public Animator legAnimator;
    public Animator mainAnimator;
    public HitCircleManager hitManager;

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize footPoint
        footPointOffset = mountTrigger.offset.y - mountTrigger.bounds.extents.y;
        footPoint = new Vector2(transform.position.x, transform.position.y + footPointOffset);

        // Initialize controls dictionary
        controls = new Dictionary<string, KeyCode>()
        {
            { "left",    leftKey    },
            { "right",   rightKey   },
            { "jump",    jumpKey    },
            { "offense", offenseKey },
            { "defense", defenseKey }
        };

        worldManager = GameObject.FindGameObjectWithTag("World").GetComponent<Manager>();
        rb = GetComponent<Rigidbody2D>();
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
        ballRb = ball.GetComponent<Rigidbody2D>();
    }





    // ---------------------------
    // - - - - - UPDATES - - - - -
    // ---------------------------

    void Update()
    {
        // Handle mobility state
        switch (moveState)
        {
            case MobilityState.Free:

                // Check which run buttons are pressed
                if (Input.GetKey(controls["right"]))
                    // In the case of both keys pressed on the same frame, default to right
                    runPressed = 'R';
                else if (Input.GetKey(controls["left"]))
                    // Go left
                    runPressed = 'L';
                else
                    // Go nowhere
                    runPressed = 'N';

                // Check if jump is initiated
                if (Input.GetKeyDown(controls["jump"]) && !airborne)
                    jumpPressed = true;

                break;


            case MobilityState.Mounting:

                runPressed = 'N';

                if (Input.GetKeyDown(controls["jump"]))
                {
                    mountJump = true;
                    jumpPressed = true;
                }

                break;



            case MobilityState.Mounted:

                runPressed = 'N';
                break;



            case MobilityState.Stunned:

                if (!airborne)
                    moveState = MobilityState.Free;

                break;



            case MobilityState.Taunting:
                break;
        }

        // Handle hit state
        switch (hitState)
        {
            case HitState.None:

                if (Input.GetKeyDown(controls["offense"]))
                {
                    hitState = HitState.Offensive;
                    hitManager.MakeAHit(hitState);
                }
                else if (Input.GetKeyDown(controls["defense"]))
                {
                    hitState = HitState.Defensive;
                    hitManager.MakeAHit(hitState);
                }
                else if (airborne && Input.GetKeyDown(controls["jump"]))
                {
                    hitState = HitState.SpikeWind;
                    hitManager.MakeAHit(hitState);
                }
                    
                break;



            case HitState.Offensive:

                hitTimer++;

                if (hitTimer > offenseDuration && !Input.GetKey(controls["offense"]))
                {
                    hitState = HitState.None;
                    hitManager.StopHitting();
                    hitTimer = 0;
                }
                
                break;



            case HitState.Defensive:

                hitTimer++;

                if (hitTimer > defenseDuration && !Input.GetKey(controls["defense"]))
                {
                    hitState = HitState.None;
                    hitManager.StopHitting();
                    hitTimer = 0;
                }

                break;



            case HitState.SpikeWind:

                hitTimer++;

                if (!airborne)
                {
                    hitState = HitState.None;
                    hitManager.StopHitting();
                    hitTimer = 0;
                }
                else if (hitTimer > spikeWindDuration && !Input.GetKey(controls["jump"]))
                {
                    hitState = HitState.SpikeHit;
                    hitManager.MakeAHit(hitState);
                    hitTimer = 0;
                }

                break;

            case HitState.SpikeHit:

                hitTimer++;

                if (!airborne || hitTimer > spikeHitDuration)
                {
                    hitState = HitState.None;
                    hitManager.StopHitting();
                    hitTimer = 0;
                }

                break;
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        // Update footPoint
        footPoint = new Vector2(transform.position.x, transform.position.y + footPointOffset);

        // Control the running
        if (runPressed == 'R')
            rb.velocity = new Vector2(runSpd, rb.velocity.y);
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
            moveState = MobilityState.Free;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (mountJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpSpd * 1.3f);
                mountJump = false;
            }
            else
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

    }

    private void UpdateAnimator()
    {
        bool isMounted = (moveState == MobilityState.Mounted);

        legAnimator.SetFloat("xVelocity", rb.velocity.x);
        legAnimator.SetBool("airborne", airborne);
        legAnimator.SetBool("mounted", isMounted);

        mainAnimator.SetBool("airborne", airborne);
        mainAnimator.SetInteger("hitState", (int)hitState);
        mainAnimator.SetBool("mounted", isMounted);
    }





    // -------------------------
    // - - - - - OTHER - - - - -
    // -------------------------

    public virtual void HitTheBall(HitCircle theBox)
    {
        // Figure out the direction the ball should be hit
        int directionX = 1;
        if (!leftSide)
            directionX = -1;

        // Calculate income variables
        Vector2 impactAngle = ballRb.velocity.normalized;
        float impactMag = ballRb.velocity.magnitude;
        float magScale = impactMag / ball.maxSpd;

        if (hitState == HitState.Offensive)
            OffenseHit(directionX, impactAngle, magScale);
        else if (hitState == HitState.Defensive)
            DefenseHit(directionX, impactAngle, magScale);
        else if (hitState == HitState.SpikeWind)
            BuntHit(directionX, impactAngle, magScale);
        else if (hitState == HitState.SpikeHit)
            SpikeHit(directionX, impactAngle, magScale);
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

        if (hitTimer <= offenseDuration)
            // Return velocity magnitude ranges from .5 -> .8 of maxSpd
            trajectory = angle * (ball.maxSpd * .5f) * (1 + (magScale * .3f));
        else
            // Return velocity magnitude ranges from .2 -> .4 of maxSpd
            trajectory = angle * (ball.maxSpd * .2f) * (1 + (magScale * .2f));

        ballRb.velocity = new Vector2(trajectory.x * directionX, trajectory.y);
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

        if (hitTimer <= defenseDuration)
            // Return velocity magnitude ranges from .4 -> .9 of maxSpd
            trajectory = angle * (ball.maxSpd * .4f) * (1 + (magScale * .5f));
        else
            // Return velocity magnitude ranges from .2 -> .5 of maxSpd
            trajectory = angle * (ball.maxSpd * .2f) * (1 + (magScale * .3f));

        ballRb.velocity = new Vector2(trajectory.x * directionX, trajectory.y);
    }

    protected virtual void BuntHit(int directionX, Vector2 impactAngle, float magScale)
    {
        // Initialize ball return variables
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        angle = Vector2.right.normalized;

        // Return velocity magnitude is .1 of maxSpd
        trajectory = angle * (ball.maxSpd * .1f);
        ballRb.velocity = new Vector2(trajectory.x * directionX, trajectory.y);
    }

    protected virtual void SpikeHit(int directionX, Vector2 impactAngle, float magScale)
    {
        // Initialize ball return variables
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        angle = Vector2.right.normalized;

        // Return velocity magnitude is .9 of maxSpd
        trajectory = angle * (ball.maxSpd * .9f);
        ballRb.velocity = new Vector2(trajectory.x * directionX, trajectory.y);
    }





    // ------------------------------
    // - - - - - COLLISIONS - - - - -
    // ------------------------------

    // Check for enter collisions with the mount trigger
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Check for collision with another athlete's hurtbox
        if (collider.gameObject.tag == "Athlete" && collider.GetType() == typeof(CapsuleCollider2D))
        {
            // Check if grounded and still
            if (!airborne && rb.velocity.x == 0)
            {
                // Get other athlete
                Athlete otherAthlete = collider.gameObject.GetComponent<Athlete>();

                // Check if other athlete is airborne and moving down
                if (otherAthlete.airborne && otherAthlete.rb.velocity.y < 0)
                {
                    // Get center point at the top semicircle of this trigger
                    float topCenterY = mountTrigger.bounds.center.y; // Get athlete y position
                    topCenterY += mountTrigger.bounds.extents.y;     // Add half the height
                    topCenterY -= mountTrigger.bounds.extents.x;     // Subtract the radius
                    Vector2 topCenter = new Vector2(mountTrigger.bounds.center.x, topCenterY);

                    // Update state and constraints
                    moveState = MobilityState.Mounted;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;

                    // Deal with other athlete
                    // THIS MUST BE DONE IN HERE. OnCollision stuff doesn't work
                    otherAthlete.transform.position = new Vector2(topCenter.x, topCenter.y + otherAthlete.mountTrigger.bounds.extents.y - otherAthlete.mountTrigger.offset.y);
                    otherAthlete.moveState = MobilityState.Mounting;
                    otherAthlete.airborne = false;
                    otherAthlete.rb.constraints = RigidbodyConstraints2D.FreezeAll;

                    // Swap layers if necessary
                    if (gameObject.layer < otherAthlete.gameObject.layer)
                    {
                        int tempLayer = gameObject.layer;
                        gameObject.layer = otherAthlete.gameObject.layer;
                        otherAthlete.gameObject.layer = tempLayer;
                    }
                }
            }
        }
    }

    // Check for exit collisions with the mount trigger
    private void OnTriggerExit2D(Collider2D collider)
    {
        // Check for collision with an Athlete's mount trigger
        if (collider.gameObject.tag == "Athlete" && collider.GetType() == typeof(CapsuleCollider2D))
        {
            // Check if currently mounted
            if (moveState == MobilityState.Mounted)
            {
                // Get other athlete
                Athlete otherAthlete = collider.gameObject.GetComponent<Athlete>();

                // Update state and constraints
                moveState = MobilityState.Free;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                // Deal with other athlete
                // THIS MUST BE DONE IN HERE. OnCollision stuff doesn't work
                otherAthlete.moveState = MobilityState.Free;
                otherAthlete.airborne = true;
                otherAthlete.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    // Check for enter collisions with the hurtbox
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D collider = collision.collider;

        // Check for collision with the floor
        if (collider.gameObject.tag == "Floor")
            airborne = false;
    }

    // Check for enter collisions with the hurtbox
    private void OnCollisionExit2D(Collision2D collision)
    {
        Collider2D collider = collision.collider;

        // Check for collision with the floor
        if (collider.gameObject.tag == "Floor")
            airborne = true;
    }
}
