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
        // Scale child sprite to fit the collider
        sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        float scale = GetComponent<CircleCollider2D>().radius/ sprite.bounds.extents.x;
        sprite.transform.localScale = new Vector2(scale, scale);

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
            floorPos = new Vector2(.2f, 3.2f);
            airPos = new Vector2(.1f, 3.8f);
            duration = .5f;
        }
        if (type == HitType.Offense)
        {
            // Offensive hit
            floorPos = new Vector2(1.1f, -1.2f);
            airPos = new Vector2(.4f, -1.4f);
            duration = .5f;
        }
        if (type == HitType.Spike)
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

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            // Enable the sprite if allowed
            if (worldManager.debugMode && !sprite.enabled)
                sprite.enabled = true;

            // If a hitbox is active, the athlete must be hitting
            if (!player.hitting)
                player.hitting = true;

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

                // Disable the sprite
                if (worldManager.debugMode || sprite.enabled)
                    sprite.enabled = false;

                // Reset player hitIndex
                player.hitting = false;
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
