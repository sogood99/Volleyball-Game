using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitType
{
    OffenseGround,
    OffenseAir,
    DefenseGround,
    DefenseAir,
    SpikeWind,
    SpikeHit,
    Serve,
    None
}

public class HitManager : MonoBehaviour
{
    // Collider Fields
    public CircleCollider2D offGrnd;
    public CircleCollider2D offAir;
    public CircleCollider2D defGrnd;
    public CircleCollider2D defAir;
    public CircleCollider2D spkWind;
    public CircleCollider2D spkHit;
    public CircleCollider2D serve;

    // Dictionary
    private Dictionary<HitType, HitCircle> circles;
    
    // Logic variables
    public HitType activeHit = HitType.None;

    // Other GameObjects
    public Athlete athlete;
    private Rigidbody2D athleteRb;

    // Start is called before the first frame update
    void Start()
    {
        // Construct the dictionary of circles
        circles = new Dictionary<HitType, HitCircle>()
        {
            { HitType.OffenseGround, offGrnd.GetComponent<HitCircle>() },
            { HitType.OffenseAir,    offAir.GetComponent<HitCircle>()  },
            { HitType.DefenseGround, defGrnd.GetComponent<HitCircle>() },
            { HitType.DefenseAir,    defAir.GetComponent<HitCircle>()  },
            { HitType.SpikeWind,     spkWind.GetComponent<HitCircle>() },
            { HitType.SpikeHit,      spkHit.GetComponent<HitCircle>()  },
            { HitType.Serve,         serve.GetComponent<HitCircle>()   }
        };

        athleteRb = athlete.gameObject.GetComponent<Rigidbody2D>();

        // Flip x offsets of hit circles
        if (!athlete.leftTeam)
        {
            foreach (KeyValuePair<HitType, HitCircle> circDef in circles)
            {
                // 2 lines is necessary please just trust me
                Vector3 circPos = circDef.Value.gameObject.transform.localPosition;
                circDef.Value.gameObject.transform.localPosition = new Vector3(-circPos.x, circPos.y, circPos.z);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Swap hits when airborne status changes
        switch (activeHit)
        {
            case HitType.OffenseGround:

                if (athlete.airborne)
                    SwapToHit(HitType.OffenseAir);

                break;


            case HitType.OffenseAir:

                if (!athlete.airborne)
                    SwapToHit(HitType.OffenseGround);

                break;


            case HitType.DefenseGround:

                if (athlete.airborne)
                    SwapToHit(HitType.DefenseAir);

                break;


            case HitType.DefenseAir:

                if (!athlete.airborne)
                    SwapToHit(HitType.DefenseGround);

                break;
        }
    }





    // ----------------------------------
    // - - - - - HIT MANAGEMENT - - - - -
    // ----------------------------------

    // Makes a hit circle based on hit state
    public void MakeAHit(HitState hitState)
    {
        switch (hitState)
        {
            case HitState.Offensive:

                if (athlete.airborne)
                    activeHit = HitType.OffenseAir;
                else
                    activeHit = HitType.OffenseGround;

                break;



            case HitState.Defensive:

                if (athlete.airborne)
                    activeHit = HitType.DefenseAir;
                else
                    activeHit = HitType.DefenseGround;

                break;



            case HitState.SpikeWind:

                activeHit = HitType.SpikeWind;

                break;



            case HitState.SpikeHit:
                // Is the activeHit change necessary?
                circles[activeHit].IsActive = false;
                activeHit = HitType.SpikeHit;
                circles[activeHit].IsActive = true;

                break;



            case HitState.Serve:

                activeHit = HitType.Serve;

                break;
        }

        circles[activeHit].IsActive = true;
    }

    // Disables any active hit circle
    public void StopHitting()
    {
        if (activeHit != HitType.None)
        {
            circles[activeHit].IsActive = false;
            activeHit = HitType.None;
        }
    }

    public void SwapToHit(HitType type)
    {
        float tempTimer = circles[activeHit].Timer;
        circles[activeHit].IsActive = false;

        activeHit = type;
        circles[activeHit].IsActive = true;
        circles[activeHit].Timer = tempTimer;
    }

    public virtual void HitTheBall(GameObject ball)
    {
        // Calculate income variables
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        Ball ballScript = ball.GetComponent<Ball>();
        Vector2 impactAngle = ballRb.velocity.normalized;
        float impactMag = ballRb.GetComponent<Rigidbody2D>().velocity.magnitude;
        float magScale = impactMag / ballScript.maxSpd;

        switch (activeHit)
        {
            case HitType.OffenseGround:
                OffenseHit(ball, impactAngle, magScale);
                break;

            case HitType.OffenseAir:
                OffenseHit(ball, impactAngle, magScale);
                break;

            case HitType.DefenseGround:
                DefenseHit(ball, impactAngle, magScale);
                break;

            case HitType.DefenseAir:
                DefenseHit(ball, impactAngle, magScale);
                break;

            case HitType.SpikeWind:
                BuntHit(ball, impactAngle, magScale);
                break;

            case HitType.SpikeHit:
                SpikeHit(ball, impactAngle, magScale);
                break;

            case HitType.Serve:
                ServeHit(ball);
                break;
        }

        if (ballScript.ballState == BallState.Held)
        {
            ballScript.ballState = BallState.Free;
            ballRb.constraints = RigidbodyConstraints2D.None;
            ballRb.gravityScale = ballScript.gravScale;
        }
    }





    // ------------------------------------
    // - - - - - HIT TYPE METHODS - - - - -
    // ------------------------------------

    // A medium, forward-leaning hit meant to return the ball
    protected virtual void OffenseHit(GameObject ball, Vector2 impactAngle, float magScale)
    {
        // Initialize variables
        Ball ballScript = ball.GetComponent<Ball>();
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        // Horizontal movement alters trajectory
        if (athleteRb.velocity.x < 0)
            angle = new Vector2(1, 1.7f).normalized;
        else if (athleteRb.velocity.x == 0)
            angle = new Vector2(1, 1.2f).normalized;
        else if (athleteRb.velocity.x > 0)
            angle = new Vector2(1, .7f).normalized;

        // Hit stronger on early contact
        if (circles[activeHit].Timer <= circles[activeHit].duration)
            // Return velocity magnitude ranges from .5 -> .8 of maxSpd
            trajectory = angle * (ballScript.maxSpd * .5f) * (1 + (magScale * .3f));
        else
            // Return velocity magnitude ranges from .2 -> .4 of maxSpd
            trajectory = angle * (ballScript.maxSpd * .2f) * (1 + (magScale * .2f));

        // Flip direction if necessary
        if (!athlete.leftTeam)
            trajectory.x *= -1;

        // Update ball velocity
        ballRb.velocity = trajectory;
    }

    // A weaker hit that pops the ball up
    protected virtual void DefenseHit(GameObject ball, Vector2 impactAngle, float magScale)
    {
        // Initialize variables
        Ball ballScript = ball.GetComponent<Ball>();
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        int athleteDirection = 1;
        // Flip direction if necessary
        if (!athlete.leftTeam)
            athleteDirection = -1;


        if (athleteRb.velocity.x < 0)
            angle = new Vector2(-.1f * athleteDirection, 1).normalized;
        else if (athleteRb.velocity.x == 0)
            angle = new Vector2(0, 1).normalized;
        else if (athleteRb.velocity.x > 0)
            angle = new Vector2(.2f * athleteDirection, 1).normalized;

        // Hit stronger on early contact
        if (circles[activeHit].Timer <= circles[activeHit].duration)
            // Return velocity magnitude ranges from .4 -> .9 of maxSpd
            trajectory = angle * (ballScript.maxSpd * .4f) * (1 + (magScale * .5f));
        else
            // Return velocity magnitude ranges from .2 -> .5 of maxSpd
            trajectory = angle * (ballScript.maxSpd * .2f) * (1 + (magScale * .3f));

        // Update ball velocity
        ballRb.velocity = trajectory;
    }

    // A very weak hit that just taps the ball forward
    protected virtual void BuntHit(GameObject ball, Vector2 impactAngle, float magScale)
    {
        // Initialize variables
        Ball ballScript = ball.GetComponent<Ball>();
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        angle = Vector2.right.normalized;

        // Return velocity magnitude is .1 of maxSpd
        trajectory = angle * (ballScript.maxSpd * .1f);

        // Flip direction if necessary
        if (!athlete.leftTeam)
            trajectory.x *= -1;

        // Update ball velocity
        ballRb.velocity = trajectory;
    }

    // A strong hit that sends the ball forward and down
    protected virtual void SpikeHit(GameObject ball, Vector2 impactAngle, float magScale)
    {
        // Initialize variables
        Ball ballScript = ball.GetComponent<Ball>();
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        angle = Vector2.right.normalized;

        // Return velocity magnitude is .9 of maxSpd
        trajectory = angle * (ballScript.maxSpd * .9f);

        // Flip direction if necessary
        if (!athlete.leftTeam)
            trajectory.x *= -1;

        // Update ball velocity
        ballRb.velocity = trajectory;
    }

    // A medium hit similar to an offense
    // Only used when holding the ball
    protected virtual void ServeHit(GameObject ball)
    {
        // Initialize variables
        Ball ballScript = ball.GetComponent<Ball>();
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        // Hit at a 45 degree angle
        angle = new Vector2(1, 1).normalized;

        // Hit ball at 80% strength
        trajectory = angle * (ballScript.maxSpd * .7f);

        // Flip direction if necessary
        if (!athlete.leftTeam)
            trajectory.x *= -1;

        // Update ball velocity
        ballRb.velocity = trajectory;
    }





    // ------------------------------
    // - - - - - COLLISIONS - - - - -
    // ------------------------------

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // I think other players can hit the ball out of your hand
        if (collider.gameObject.tag == "Ball" && collider.gameObject.GetComponent<Ball>().ballState != BallState.Grounded)
            HitTheBall(collider.gameObject);
    }





    // -------------------------------
    // - - - - - INFORMATION - - - - -
    // -------------------------------

    // Returns the offset of the circle collider's center.
    public Vector3 GetHitOffset(HitType type)
    {
        if (type == HitType.OffenseGround)
            return offGrnd.transform.localPosition;
        else if (type == HitType.OffenseAir)
            return offAir.transform.localPosition;
        else if (type == HitType.DefenseGround)
            return defGrnd.transform.localPosition;
        else if (type == HitType.DefenseAir)
            return defAir.transform.localPosition;
        else if (type == HitType.SpikeWind)
            return spkWind.transform.localPosition;
        else if (type == HitType.SpikeHit)
            return spkHit.transform.localPosition;
        else if (type == HitType.Serve)
            return serve.transform.localPosition;
        else
            return Vector3.zero;
    }

    // Returns a hit circle.
    public HitCircle GetHitCircle(HitType type)
    {
        return circles[type];
    }
}
