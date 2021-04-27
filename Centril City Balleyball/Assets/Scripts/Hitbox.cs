using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public Player player;
    public Ball ball;

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
        ball = null;
    }

    // Parameterized constructor
    public Hitbox(int type, Ball ball = null)
    {
        if (type == 0)
        {
            // Defensive hit
            floorPos = new Vector2(0f, .5f);
            airPos = new Vector2(0f, .8f);
            duration = .5f;
        }
        if (type == 1)
        {
            // Offensive hit
            floorPos = new Vector2(.6f, -.2f);
            airPos = new Vector2(.3f, -.1f);
            duration = .5f;
        }
        if (type == 2)
        {
            // Spike Hit
            floorPos = new Vector2(.7f, .1f);
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

        this.ball = ball;
    }

    // VERY parameterized constructor
    public Hitbox(Vector2 floorPos, Vector2 airPos, float duration, Ball ball = null)
    {
        this.floorPos = floorPos;
        this.airPos = airPos;
        this.duration = duration;
        this.ball = ball;
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

                // Disable the sprite (for debugging)
                GetComponent<SpriteRenderer>().enabled = false;

                // Reset player hitIndex
                player.hitIndex = -1;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (active && collider.gameObject == ball.gameObject)
        {
            Debug.Log("hello");
            if (type == 0)
                ball.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 60), ForceMode2D.Impulse);
            else if (type == 1)
                ball.GetComponent<Rigidbody2D>().AddForce(new Vector2(30, 30), ForceMode2D.Impulse);
            else if (type == 2)
                ball.GetComponent<Rigidbody2D>().AddForce(new Vector2(40, -10), ForceMode2D.Impulse);
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
