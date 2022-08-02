using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public enum BallState
{
    Held,
    Free,
    Grounded
}

public class Ball : MonoBehaviour
{
    // Logic variables
    public float maxSpd;
    public float gravScale;
    public Vector2 startPos;
    private Vector2 direction;
    public BallState ballState;
    public Athlete beholder;
    private float groundTimer;
    private float groundDuration = 2;

    // Component variables
    private Rigidbody2D rb;
    private Transform sprite;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravScale = rb.gravityScale;
        rb.gravityScale = 0;
        sprite = transform.GetChild(0);

        //transform.position = startPos;
    }

    // Update is called once per frame
    void Update()
    {
        // The movement is jittery if done in FixedUpdate
        if (ballState == BallState.Held)
            transform.position = beholder.transform.position + beholder.hitManager.GetHitOffset(HitType.Serve);
    }

    // Put all of the rigidbody stuff in here
    private void FixedUpdate()
    {
        switch (ballState)
        {
            case BallState.Held:

                break;



            case BallState.Free:

                // Direction reflects angle of velocity vector
                direction = rb.velocity.normalized;

                // Ball sprite rotation should match direction
                sprite.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity.normalized);

                // Stretch the ball based on speed
                if (rb.velocity.magnitude >= maxSpd * .4f)
                {
                    float surplusSpd = rb.velocity.magnitude - (maxSpd * .4f);
                    float stretch = surplusSpd / (maxSpd * .6f);
                    // Stretches from 1 -> 2, Squashes from 1 -> 1.5
                    sprite.localScale = new Vector2(1 - (stretch / 2), 1 + stretch);
                }
                else
                    sprite.localScale = new Vector2(1, 1);

                // Clamp the velocity magnitude
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpd);

                break;



            case BallState.Grounded:

                groundTimer += Time.deltaTime;

                Debug.Log("Ground Timer: " + groundTimer);

                if (groundTimer >= groundDuration)
                {
                    groundTimer = 0;

                    // Invoke an event
                    if (OnBallLanded != null)
                        OnBallLanded();
                }

                break;
        }
    }

    // Resets the ball
    public void Respawn(Athlete beholder)
    {
        ballState = BallState.Held;
        this.beholder = beholder;

        rb.constraints = RigidbodyConstraints2D.None;
        rb.gravityScale = 0;

        sprite.GetComponent<SpriteRenderer>().color = Color.white;
        sprite.position = new Vector3(sprite.position.x, sprite.position.y, sprite.position.z - .1f);
    }

    // Event

    public delegate void BallLanded();
    public static event BallLanded OnBallLanded;



    // Deal with collisions
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Check for floor collision
        if (collision.gameObject.name == "Floor" && ballState == BallState.Grounded)
        {
            sprite.GetComponent<SpriteRenderer>().color = Color.gray;
            sprite.position = new Vector3(sprite.position.x, sprite.position.y, sprite.position.z + .1f);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for floor collision
        if (collision.gameObject.name == "Floor")
        {
            // Set hitGround to true on initial bounce
            if (ballState == BallState.Free)
            {
                ballState = BallState.Grounded;
                sprite.localScale = new Vector2(1, 1);
            }
            // On second contact, freeze position
            // This prevents infinite bouncing
            else
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

}
