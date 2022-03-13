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
    None
}

public class HitCircleManager : MonoBehaviour
{
    // Collider Fields
    public CircleCollider2D offGrnd;
    public CircleCollider2D offAir;
    public CircleCollider2D defGrnd;
    public CircleCollider2D defAir;
    public CircleCollider2D spkWind;
    public CircleCollider2D spkHit;

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
            { HitType.SpikeHit,      spkHit.GetComponent<HitCircle>()  }
        };

        athleteRb = athlete.gameObject.GetComponent<Rigidbody2D>();
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

                circles[activeHit].IsActive = false;
                activeHit = HitType.SpikeHit;
                circles[activeHit].IsActive = true;

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


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Ball")
            HitTheBall(collider.gameObject);
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
        }
    }

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
        if (!athlete.leftSide)
            trajectory.x *= -1;

        // Update ball velocity
        ballRb.velocity = trajectory;
    }

    protected virtual void DefenseHit(GameObject ball, Vector2 impactAngle, float magScale)
    {
        // Initialize variables
        Ball ballScript = ball.GetComponent<Ball>();
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        Vector2 angle = Vector2.zero;
        Vector2 trajectory = Vector2.zero;

        if (athleteRb.velocity.x < 0)
            angle = new Vector2(-.1f, 1).normalized;
        else if (athleteRb.velocity.x == 0)
            angle = new Vector2(0, 1).normalized;
        else if (athleteRb.velocity.x > 0)
            angle = new Vector2(.2f, 1).normalized;

        // Hit stronger on early contact
        if (circles[activeHit].Timer <= circles[activeHit].duration)
            // Return velocity magnitude ranges from .4 -> .9 of maxSpd
            trajectory = angle * (ballScript.maxSpd * .4f) * (1 + (magScale * .5f));
        else
            // Return velocity magnitude ranges from .2 -> .5 of maxSpd
            trajectory = angle * (ballScript.maxSpd * .2f) * (1 + (magScale * .3f));

        // Flip direction if necessary
        if (!athlete.leftSide)
            trajectory.x *= -1;

        // Update ball velocity
        ballRb.velocity = trajectory;
    }

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
        if (!athlete.leftSide)
            trajectory.x *= -1;

        // Update ball velocity
        ballRb.velocity = trajectory;
    }

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
        if (!athlete.leftSide)
            trajectory.x *= -1;

        // Update ball velocity
        ballRb.velocity = trajectory;
    }
}
