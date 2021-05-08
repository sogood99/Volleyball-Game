using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitType
{
    Defense,
    Offense,
    Spike
}

public class Hitbox : MonoBehaviour
{
    // Personalized variables
    public Vector2 floorPos;
    public Vector2 airPos;
    public float duration;

    // Logic variables
    public HitType type;
    public bool active = false;
    private bool sameHit = false;
    private float timer = 0;

    // Other GameObjects
    private Manager worldManager;
    public Athlete player;
    public SpriteRenderer sprite;



    // Start is called before the first frame update
    void Start()
    {
        // Find link up worldManager
        worldManager = GameObject.FindGameObjectWithTag("World").GetComponent<Manager>();
    }

    // Default constructor
    public Hitbox()
    {
        floorPos = Vector2.zero;
        airPos = Vector2.zero;
    }

    // Parameterized constructor
    public Hitbox(HitType type)
    {
        if (type == HitType.Defense)
        {
            // Defensive hit
            floorPos = new Vector2(0f, 1.4f);
            airPos = new Vector2(.05f, 1.8f);
            duration = .2f;
        }
        if (type == HitType.Offense)
        {
            // Offensive hit
            floorPos = new Vector2(.9f, -.3f);
            airPos = new Vector2(.15f, -.5f);
            duration = .2f;
        }
        if (type == HitType.Spike)
        {
            // Spike Hit
            floorPos = new Vector2(1.4f, .2f);
            airPos = floorPos;
            duration = .2f;
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

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            // If a hitbox is active, the athlete must be hitting
            if (!player.hitting)
                player.hitting = true;

            // Fix position if necessary
            CorrectPosition(player.airborne);

            // A spike immediately stops once the athlete lands
            if (type == HitType.Spike && !player.airborne)
                timer = duration;

            // Add to timer
            timer += Time.deltaTime;

            // End active state when time is up
            if (timer >= duration)
            {
                timer = 0;
                active = false;
                sameHit = false;

                // Reset player hitIndex
                player.hitting = false;
                player.activeHitbox = -1;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (active && collider.gameObject == player.ballRb.gameObject)
            if (!sameHit && !player.ball.hitGround)
            {
                player.HitTheBall(this);

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
