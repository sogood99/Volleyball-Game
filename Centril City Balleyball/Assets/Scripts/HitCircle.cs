using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCircle : MonoBehaviour
{
    // Fields
    public float radiusScale;
    public float duration;
    public CircleCollider2D circCollider;

    private bool active = false;
    private float ogRadius;
    private float timer;



    // Properties
    public bool IsActive { 
        get { return active; }
        set
        {
            active = value;
            circCollider.enabled = value;

            if (value == true)
            {
                timer = 0;
                if (timer <= duration)
                    circCollider.radius = ogRadius * radiusScale;
            }
            if (value == false)
            {
                timer = 0;
                circCollider.radius = ogRadius;
            }
        }
    }
    public float Timer {
        get { return timer; }
        set { 
            timer = value;

            if (timer > duration)
                circCollider.radius = ogRadius;
        } 
    }

    // Start is called before the first frame update
    void Start()
    {
        ogRadius = circCollider.radius;
        timer = 0;
        circCollider.radius = ogRadius * radiusScale;

        circCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (active && circCollider.radius != ogRadius)
        {
            timer++;

            if (timer > duration)
                circCollider.radius = ogRadius;
        }
    }
}
