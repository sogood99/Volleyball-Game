using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody2D rb;

    public float maxSpd = 70;

    private Transform sprite;
    private Vector2 direction;

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
        if (rb.velocity.magnitude >= maxSpd/2)
        {
            float surplusSpd = rb.velocity.magnitude - (maxSpd/2);
            float stretch = surplusSpd / (maxSpd/2);
            sprite.localScale = new Vector2(1 - (stretch * 1/3), 1 + stretch);
        }
        else
            sprite.localScale = new Vector2(1, 1);
    }

    // Put all of the rigidbody stuff in here
    private void FixedUpdate()
    {
        // Clamp the velocity magnitude
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpd);
    }
}
