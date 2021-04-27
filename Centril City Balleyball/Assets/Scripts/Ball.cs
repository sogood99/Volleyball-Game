using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody2D rb;

    public float maxSpd = 150;

    private Transform sprite;
    private Vector2 direction;

    public bool hitGround = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
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
            if (sprite.localScale != new Vector3(1, 1, 1))
                sprite.localScale = new Vector2(1, 1);
    }

    // Put all of the rigidbody stuff in here
    private void FixedUpdate()
    {
        // Clamp the velocity magnitude
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpd);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            hitGround = true;
            sprite.GetComponent<SpriteRenderer>().color = Color.gray;
            sprite.position = new Vector3(sprite.position.x, sprite.position.y, 4.5f);
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
