using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public GameObject ball;

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
    public Hitbox(int type, GameObject ball = null)
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
    public Hitbox(Vector2 floorPos, Vector2 airPos, float duration, GameObject ball = null)
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
            // Add to timer
            timer += Time.deltaTime;

            // End active state when time is up
            if (timer >= duration)
            {
                timer = 0;
                active = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (active && collision.gameObject == ball)
        {
            if (type == 0)
                ball.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 60), ForceMode2D.Impulse);
            else if (type == 1)
                ball.GetComponent<Rigidbody2D>().AddForce(new Vector2(30, 30), ForceMode2D.Impulse);
            else if (type == 2)
                ball.GetComponent<Rigidbody2D>().AddForce(new Vector2(40, -10), ForceMode2D.Impulse);
        }
    }
}
