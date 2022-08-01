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

public enum LegAnimState
{
    None,
    Idle,
    RunningForward,
    RunningBackward
}

public class Athlete : MonoBehaviour
{
    // ----------------------------------
    // - - - - - INITIALIZATION - - - - -
    // ----------------------------------

    // Control Fields
    public bool controllable = false;
    private Dictionary<string, KeyCode> playerControls;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
    public KeyCode offenseKey;
    public KeyCode defenseKey;

    // AI Control Fields
    public bool aI = false;
    private Dictionary<string, bool> aiControls;
    private bool aiLeftKey    = false;
    private bool aiRightKey   = false;
    private bool aiJumpKey    = false;
    private bool aiOffenseKey = false;
    private bool aiDefenseKey = false;
    private HitType predictedHit = HitType.DefenseGround;

    // Hit Fields
    private int hitTimer = 0;
    private int offenseDuration = 60;
    private int defenseDuration = 60;
    private int spikeWindDuration = 5;
    private int spikeHitDuration = 20;
    private bool didASpike = false;

    // Logic fields
    public float runSpd;
    public float jumpSpd;
    public bool leftTeam;
    public float maxGravScale;

    // State fields
    public MobilityState moveState;
    public HitState hitState;
    public LegAnimState legAnimState;
    public bool airborne;
    public bool mountJump = false;
    public bool serving;

    // Communication fields
    private char runPressed;
    private bool jumpPressed = false;

    // Hurtbox Fields
    private Rigidbody2D rb;
    public BoxCollider2D hurtbox;
    public CapsuleCollider2D mountTrigger;

    // Component fields
    public GameObject legs;
    public GameManager worldManager;
    public Animator legAnimator;
    public Animator mainAnimator;
    public HitManager hitManager;

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize controls dictionary
        playerControls = new Dictionary<string, KeyCode>()
        {
            { "left",    leftKey    },
            { "right",   rightKey   },
            { "jump",    jumpKey    },
            { "offense", offenseKey },
            { "defense", defenseKey }
        };
        // Initialize AI controls dictionary
        aiControls = new Dictionary<string, bool>()
        {
            { "left",    aiLeftKey    },
            { "right",   aiRightKey   },
            { "jump",    aiJumpKey    },
            { "offense", aiOffenseKey },
            { "defense", aiDefenseKey }
        };

        worldManager = GameObject.FindGameObjectWithTag("World").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody2D>();

        maxGravScale = rb.gravityScale;

        if (!leftTeam)
        {
            // Flip main and leg sprite
            gameObject.GetComponent<SpriteRenderer>().flipX = true;
            transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;
        }
    }





    // ---------------------------
    // - - - - - UPDATES - - - - -
    // ---------------------------

    void Update()
    {
        // Work on making simultaneous player and AI control

        if (controllable)
        {
            HandleMoveState(
                Input.GetKey(playerControls["right"]),
                Input.GetKey(playerControls["left"]),
                Input.GetKeyDown(playerControls["jump"])
                );

            HandleHitState(
                Input.GetKey(playerControls["offense"]),
                Input.GetKey(playerControls["defense"]),
                Input.GetKeyDown(playerControls["jump"]),
                Input.GetKey(playerControls["jump"])
                );
        }
        else if (aI)
        {
            Think(GameObject.FindWithTag("Ball").GetComponent<Ball>());

            HandleMoveState(
                aiControls["right"],
                aiControls["left"],
                aiControls["jump"]
                );

            HandleHitState(
                aiControls["offense"],
                aiControls["defense"],
                aiControls["jump"],
                aiControls["jump"]
                );

            // Reset all AI controls to false
            // Create a temporary list because modifying the controls thing sucks
            foreach (string name in new List<string>(aiControls.Keys))
                aiControls[name] = false;
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        // Control the running
        if (leftTeam)
        {
            if (runPressed == 'R')
                rb.velocity = new Vector2(runSpd, rb.velocity.y);
            else if (runPressed == 'L')
                rb.velocity = new Vector2(-runSpd * 4 / 5, rb.velocity.y);
            else
                rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            if (runPressed == 'R')
                rb.velocity = new Vector2(runSpd * 4 / 5, rb.velocity.y);
            else if (runPressed == 'L')
                rb.velocity = new Vector2(-runSpd, rb.velocity.y);
            else
                rb.velocity = new Vector2(0, rb.velocity.y);
        }


        // Move slower in the air
        if (airborne)
            rb.velocity = new Vector2(rb.velocity.x * .9f, rb.velocity.y);
        // Upwards velocity for the jump
        else if (jumpPressed)
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

            Debug.Log("Jumped");
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
        if (!airborne)
        {
            if (rb.velocity.x == 0)
            {
                legAnimState = LegAnimState.Idle;
            }
            else
            {
                if (leftTeam)
                {
                    if (rb.velocity.x > 0)
                        legAnimState = LegAnimState.RunningForward;
                    else
                        legAnimState = LegAnimState.RunningBackward;
                }
                else
                {
                    if (rb.velocity.x > 0)
                        legAnimState = LegAnimState.RunningBackward;
                    else
                        legAnimState = LegAnimState.RunningForward;
                }
            }
        }
        else
        {
            legAnimState = LegAnimState.None;
        }



        bool isMounted = (moveState == MobilityState.Mounted);

        legAnimator.SetFloat("xVelocity", rb.velocity.x);
        legAnimator.SetBool("airborne", airborne);
        legAnimator.SetInteger("hitState", (int)hitState);
        legAnimator.SetBool("mounted", isMounted);
        legAnimator.SetInteger("legAnimState", (int)legAnimState);

        mainAnimator.SetBool("airborne", airborne);
        mainAnimator.SetInteger("hitState", (int)hitState);
        mainAnimator.SetBool("mounted", isMounted);
    }





    // --------------------------
    // - - - - - STATES - - - - -
    // --------------------------

    private void HandleMoveState(bool rightInput, bool leftInput, bool firstJumpInput)
    {
        switch (moveState)
        {
            case MobilityState.Free:

                // Check which run buttons are pressed
                if (rightInput)
                {
                    // In the case of both keys pressed on the same frame, default to right
                    runPressed = 'R';
                }
                else if (leftInput)
                {
                    // Go left
                    runPressed = 'L';
                }
                else
                {
                    // Go nowhere
                    runPressed = 'N';

                    legAnimState = LegAnimState.Idle;
                }

                // Check if jump is initiated
                if (firstJumpInput && !airborne)
                {
                    jumpPressed = true;
                }


                break;


            case MobilityState.Mounting:

                runPressed = 'N';

                if (firstJumpInput)
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
    }
    private void HandleHitState(bool offenseInput, bool defenseInput, bool firstJumpInput, bool jumpInput)
    {
        switch (hitState)
        {
            case HitState.None:

                if (offenseInput)
                {
                    hitState = HitState.Offensive;
                    hitManager.MakeAHit(hitState);
                }
                else if (defenseInput)
                {
                    hitState = HitState.Defensive;
                    hitManager.MakeAHit(hitState);
                }
                else if (airborne && firstJumpInput && !didASpike)
                {
                    hitState = HitState.SpikeWind;
                    hitManager.MakeAHit(hitState);

                    rb.AddForce(new Vector2(0, 1000), ForceMode2D.Impulse);
                }

                break;



            case HitState.Offensive:

                hitTimer++;

                if (hitTimer > offenseDuration && !offenseInput)
                {
                    hitState = HitState.None;
                    hitManager.StopHitting();
                    hitTimer = 0;
                }

                break;



            case HitState.Defensive:

                hitTimer++;

                if (hitTimer > defenseDuration && !defenseInput)
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
                else if (hitTimer > spikeWindDuration && !jumpInput)
                {
                    hitState = HitState.SpikeHit;
                    hitManager.MakeAHit(hitState);
                    hitTimer = 0;

                    didASpike = true;
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

        if (moveState == MobilityState.Stunned || moveState == MobilityState.Mounted)
        {
            hitState = HitState.None;
            hitManager.StopHitting();
        }
    }





    // -------------------------
    // - - - - - OTHER - - - - -
    // -------------------------

    private void Think(Ball ball)
    {
        if (!ball.hitGround)
        {
            // STEP 1 : Predict the ball's trajectory using physics
            // -------------------------------------------------
            // *The ball doesn't exactly follow physics so it's imperfect

            // Givens
            Vector2 ballPos = ball.transform.position;
            Vector2 ballVel = ball.GetComponent<Rigidbody2D>().velocity;
            Vector2 acceleration = new Vector2(Physics.gravity.x, Physics.gravity.y * ball.GetComponent<Rigidbody2D>().gravityScale);
            float ballDrag = ball.GetComponent<Rigidbody2D>().drag;
            Vector2 hitPos = transform.position + hitManager.GetHitOffset(predictedHit);

            // Calculate predicted final velocity
            // Vertical:   Vf^2 - Vi^2 = 2*a*d   ->   Vf = sqrt( Vi^2 + (2*a*d) )
            float totalYDist = hitPos.y - ballPos.y;                                                 // This should always be negative
            float predBallVelY = -Mathf.Sqrt(Mathf.Pow(ballVel.y, 2) + (2 * acceleration.y * totalYDist)); // Just make the value negative
            Vector2 predBallVel = new Vector2(ballVel.x, predBallVelY);

            // Calculate predicted total time 
            // Vf = Vi + (a*t)   ->   t = (Vf - Vi) / a
            float predBallTime = (predBallVelY - ballVel.y) / acceleration.y;

            // Calculate predicted distance
            // Horizontal:   v = d / t   ->   d = v * t
            float predBallDistX = ballVel.x * predBallTime;
            // Vertical:   d = (Vi*t) + (.5*a*(t^2))
            float predBallDistY = (ballVel.y * predBallTime) + (.5f * (acceleration.y - ballDrag) * Mathf.Pow(predBallTime, 2));
            Vector2 predBallDist = new Vector2(predBallDistX, predBallDistY);

            // Calculate predicted position
            float predBallPosX = ballPos.x + predBallDistX;
            float predBallPosY = ballPos.y + predBallDistY;
            Vector2 predBallPos = new Vector2(predBallPosX, predBallPosY);

            // Debugging logs
            // Debug.Log("Current Velocity: " + ballVel + " | Predicted Velocity: " + predBallVel + " | Predicted Time: " + predBallTime);
            // Debug.Log("Predicted Ball X Position: " + predBallPosX);
            // Debug.Log("Current Position: " + ballPos + " | Predicted Distance: " + predBallDist + " | Predicted Position: " + predBallPos);

            GameObject debug = GameObject.Find("Thought Dots");
            for (int i = 0; i < debug.transform.childCount; i++)
            {
                // Calculate time interval
                float predTimeInterval = predBallTime * ((1 + i) / (float)debug.transform.childCount);
                // Debug.Log("Time interval " + i + ": " + timeInterval);
            
                // Place the dots
                debug.transform.GetChild(i).position = new Vector3(
                    ballPos.x + (ballVel.x * predTimeInterval),
                    ballPos.y + ( (ballVel.y * predTimeInterval) + (.5f * (acceleration.y - ballDrag) * Mathf.Pow(predTimeInterval, 2)) ),
                    0
                    );
            }



            // STEP 2 : Decide whether or not to care
            // --------------------------------------

            if (leftTeam && predBallPos.x <= 0)
            {
                DecideMovement(predBallPos.x, hitPos.x);
                DecideHit(ball);
            }
            else if (!leftTeam && predBallPos.x >= 0)
            {
                DecideMovement(predBallPos.x, hitPos.x);
                DecideHit(ball);
            }
        }
    }

    private void DecideMovement(float finalBallPosX, float offGrndHitPosX)
    {
        Vector2 hitCirclePos = transform.position + hitManager.GetHitOffset(predictedHit);
        //Debug.Log("Athlete Position: " + transform.position + " | HitCircle Offset: " + hitManager.GetHitOffset(predictedHit) + " | HitCircle Position: " + hitCirclePos);

        if (finalBallPosX < offGrndHitPosX - hitManager.GetHitCircle(predictedHit).OgRadius)
        {
            aiControls["left"] = true;
            //Debug.Log("Moving Left");
        }
        else if (finalBallPosX > offGrndHitPosX + hitManager.GetHitCircle(predictedHit).OgRadius)
        {
            aiControls["right"] = true;
            //Debug.Log("Moving Right");
        }
        else
        {
            //Debug.Log("Staying Still");
            return;
        }
    }
    private void DecideHit(Ball ball)
    {
        // Gather info
        Vector2 ballPos = ball.transform.position;
        Vector2 hitPos = transform.position + hitManager.GetHitOffset(predictedHit);
        float ballRad = ball.GetComponent<CircleCollider2D>().radius;
        float ogHitRad = hitManager.GetHitCircle(predictedHit).OgRadius;

        // Calculate ball and hitcircle distance squared
        float distSquared = Mathf.Pow(ballPos.x - hitPos.x, 2) + Mathf.Pow(ballPos.y - hitPos.y, 2);
        // Calculate radii sum squared
        float radiiSquared = Mathf.Pow(ballRad + ogHitRad, 2);

        // If within range, hit it
        if (distSquared <= radiiSquared)
        {
            if (predictedHit == HitType.OffenseGround)
                aiControls["offense"] = true;
            else if (predictedHit == HitType.DefenseGround)
                aiControls["defense"] = true;

            // Choose next hit type
            int roll = Random.Range(1, 3);
            if (roll == 1)
                predictedHit = HitType.OffenseGround;
            else
                predictedHit = HitType.DefenseGround;
        }
    }





    // ------------------------------
    // - - - - - COLLISIONS - - - - -
    // ------------------------------

    // Check for enter collisions with the mount trigger
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Check for collision with an athlete's hurtbox
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
                    otherAthlete.transform.position = new Vector3(
                        topCenter.x, 
                        topCenter.y + otherAthlete.mountTrigger.bounds.extents.y - otherAthlete.mountTrigger.offset.y, 
                        otherAthlete.transform.position.z
                        );
                    otherAthlete.moveState = MobilityState.Mounting;
                    otherAthlete.airborne = false;
                    otherAthlete.didASpike = false;
                    otherAthlete.rb.constraints = RigidbodyConstraints2D.FreezeAll;

                    // Swap layers if necessary
                    if (gameObject.layer < otherAthlete.gameObject.layer)
                    {
                        float tempLayer = transform.position.z;

                        transform.position = new Vector3(transform.position.x, transform.position.y, otherAthlete.transform.position.z);
                        legs.transform.position = new Vector3(legs.transform.position.x, legs.transform.position.y, transform.position.z - .5f);

                        otherAthlete.transform.position = new Vector3(otherAthlete.transform.position.x, otherAthlete.transform.position.y, tempLayer);
                        otherAthlete.legs.transform.position = new Vector3(otherAthlete.legs.transform.position.x, otherAthlete.legs.transform.position.y, tempLayer - .5f);
                    }
                }
            }
        }
    }

    // Check for exit collisions with the mount trigger
    private void OnTriggerExit2D(Collider2D collider)
    {
        // Check for collision with an athlete's mount trigger
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
        {
            airborne = false;
            didASpike = false;
        }
    }

    // Check for exit collisions with the hurtbox
    private void OnCollisionExit2D(Collision2D collision)
    {
        Collider2D collider = collision.collider;

        // Check for collision with the floor
        if (collider.gameObject.tag == "Floor")
        {
            airborne = true;
        }
    }
}
