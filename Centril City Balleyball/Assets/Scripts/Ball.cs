using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Logic variables
    public float maxSpd;
    public Vector2 startPos;
    private bool hitGround = false;
    private Vector2 direction;


    // Component variables
    private Rigidbody2D rb;
    private Transform sprite;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = transform.GetChild(0);

        transform.position = startPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.constraints == RigidbodyConstraints2D.FreezeAll)
            Respawn();
    }

    // Put all of the rigidbody stuff in here
    private void FixedUpdate()
    {
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
    }

    // Resets the ball
    private void Respawn()
    {
        transform.position = startPos;
        hitGround = false;

        sprite.GetComponent<SpriteRenderer>().color = Color.white;
        sprite.position = new Vector3(sprite.position.x, sprite.position.y, sprite.position.z - .1f);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            hitGround = true;
            sprite.GetComponent<SpriteRenderer>().color = Color.gray;
            sprite.position = new Vector3(sprite.position.x, sprite.position.y, sprite.position.z + .1f);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor" && hitGround)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

}
