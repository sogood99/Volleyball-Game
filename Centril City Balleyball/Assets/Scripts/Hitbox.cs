using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player player;
    private bool sameHit = false;

    public Vector2 floorPos;
    public Vector2 airPos;

    public int type;

    public float timer = 0;
    public float duration;

    public bool active = false;

    // Default constructor
    public Hitbox()
    {
        floorPos = Vector2.zero;
        airPos = Vector2.zero;
    }

    // Parameterized constructor
    public Hitbox(int type)
    {
        if (type == 0)
        {
            // Defensive hit
            floorPos = new Vector2(.2f, 3.2f);
            airPos = new Vector2(.1f, 3.8f);
            duration = .5f;
        }
        if (type == 1)
        {
            // Offensive hit
            floorPos = new Vector2(1.1f, -1.2f);
            airPos = new Vector2(.4f, -1.4f);
            duration = .5f;
        }
        if (type == 2)
        {
            // Spike Hit
            floorPos = new Vector2(2.4f, .5f);
            airPos = floorPos;
            duration = .5f;
        }
        else
        {
            // Default dummy hit
            floorPos = Vector2.zero;
            airPos = Vector2.zero;
            duration = 0f;
        }
    }

    // VERY parameterized constructor
    public Hitbox(Vector2 floorPos, Vector2 airPos, float duration)
    {
        this.floorPos = floorPos;
        this.airPos = airPos;
        this.duration = duration;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            // Enable the sprite (for debugging)
            if (!GetComponent<SpriteRenderer>().enabled)
                GetComponent<SpriteRenderer>().enabled = true;

            // Fix position if necessary
            CorrectPosition(player.airborne);

            // Add to timer
            timer += Time.deltaTime;

            // End active state when time is up
            if (timer >= duration)
            {
                timer = 0;
                active = false;
                sameHit = false;

                // Disable the sprite (for debugging)
                GetComponent<SpriteRenderer>().enabled = false;

                // Reset player hitIndex
                player.hitIndex = -1;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (active && collider.gameObject == player.ballRb.gameObject)
            if (!sameHit && !player.ball.hitGround)
        {
            // Figure out the direction the ball should be hit
            int returnDirection = 1;
            if (!player.leftSide)
                returnDirection = -1;

            // Initialize return variables
            Vector2 angle = Vector2.zero;
            Vector2 trajectory = Vector2.zero;

            // Calculate income variables
            Vector2 impactAngle = player.ballRb.velocity.normalized;
            float impactMag = player.ballRb.velocity.magnitude;
            float magScale = impactMag / player.ball.maxSpd;

            // Figure out how the ball should be returned
            if (type == 0)
            {
                if (player.GetComponent<Rigidbody2D>().velocity.x < 0)
                    angle = new Vector2(-.1f, 1).normalized;
                else if (player.GetComponent<Rigidbody2D>().velocity.x == 0)
                    angle = new Vector2(0, 1).normalized;
                else
                    angle = new Vector2(.2f, 1).normalized;

                // Return velocity magnitude ranges from .4 -> .9 of maxSpd
                trajectory = angle * (player.ball.maxSpd * .4f) * (1 + (magScale * .5f));
            }
            else if (type == 1)
            {
                if (player.GetComponent<Rigidbody2D>().velocity.x < 0)
                    angle = new Vector2(1, 1.7f).normalized;
                else if (player.GetComponent<Rigidbody2D>().velocity.x == 0)
                    angle = new Vector2(1, 1.2f).normalized;
                else
                    angle = new Vector2(1, .7f).normalized;

                // Return velocity magnitude ranges from .5 -> .8 of maxSpd
                trajectory = angle * (player.ball.maxSpd * .5f) * (1 + (magScale * .3f));
            }
            else if (type == 2)
            {
                angle = new Vector2(1, -1.4f).normalized;

                // Return velocity magnitude is .9 of maxSpd
                trajectory = angle * (player.ball.maxSpd * .9f);
            }

            player.ballRb.velocity = new Vector2(trajectory.x * returnDirection, trajectory.y);

            // This makes sure a hitbox only acts once
            sameHit = true;
        }
    }

    // Change position if it doesn't fit with player
    public void CorrectPosition(bool airborne)
    {
        if (airborne && transform.localPosition != new Vector3(airPos.x, airPos.y, 0))
            transform.localPosition = airPos;
        else if (!airborne && transform.localPosition != new Vector3(floorPos.x, floorPos.y, 0))
            transform.localPosition = floorPos;
    }
}
