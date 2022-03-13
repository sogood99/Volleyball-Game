using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitType
{
    OffenseGround,
    OffenseAir,
    DefenseGround,
    DefenseAir,
    SpikeWind,
    SpikeHit,
    None
}

public class HitCircleManager : MonoBehaviour
{
    // Collider Fields
    public CircleCollider2D offGrnd;
    public CircleCollider2D offAir;
    public CircleCollider2D defGrnd;
    public CircleCollider2D defAir;
    public CircleCollider2D spkWind;
    public CircleCollider2D spkHit;

    // Dictionary
    private Dictionary<HitType, HitCircle> circles;
    
    // Logic variables
    public HitType activeHit = HitType.None;

    // Other GameObjects
    public Athlete athlete;

    // Start is called before the first frame update
    void Start()
    {
        // Construct the dictionary of circles
        circles = new Dictionary<HitType, HitCircle>()
        {
            { HitType.OffenseGround, offGrnd.GetComponent<HitCircle>() },
            { HitType.OffenseAir,    offAir.GetComponent<HitCircle>()  },
            { HitType.DefenseGround, defGrnd.GetComponent<HitCircle>() },
            { HitType.DefenseAir,    defAir.GetComponent<HitCircle>()  },
            { HitType.SpikeWind,     spkWind.GetComponent<HitCircle>() },
            { HitType.SpikeHit,      spkHit.GetComponent<HitCircle>()  }
        };

        Debug.Log(offGrnd);
    }

    // Update is called once per frame
    void Update()
    {
        // Swap hits when airborne status changes
        switch (activeHit)
        {
            case HitType.OffenseGround:

                if (athlete.airborne)
                    SwapToHit(HitType.OffenseAir);

                break;


            case HitType.OffenseAir:

                if (!athlete.airborne)
                    SwapToHit(HitType.OffenseGround);

                break;


            case HitType.DefenseGround:

                if (athlete.airborne)
                    SwapToHit(HitType.DefenseAir);

                break;


            case HitType.DefenseAir:

                if (!athlete.airborne)
                    SwapToHit(HitType.DefenseGround);

                break;
        }
    }

    // Makes a hit circle based on hit state
    public void MakeAHit(HitState hitState)
    {
        switch (hitState)
        {
            case HitState.Offensive:

                if (athlete.airborne)
                    activeHit = HitType.OffenseAir;
                else
                    activeHit = HitType.OffenseGround;

                break;



            case HitState.Defensive:

                if (athlete.airborne)
                    activeHit = HitType.DefenseAir;
                else
                    activeHit = HitType.DefenseGround;

                break;



            case HitState.SpikeWind:

                activeHit = HitType.SpikeWind;

                break;



            case HitState.SpikeHit:

                circles[activeHit].IsActive = false;
                activeHit = HitType.SpikeHit;
                circles[activeHit].IsActive = true;

                break;
        }

        circles[activeHit].IsActive = true;
    }

    // Disables any active hit circle
    public void StopHitting()
    {
        if (activeHit != HitType.None)
        {
            circles[activeHit].IsActive = false;
            activeHit = HitType.None;
        }
    }

    public void SwapToHit(HitType type)
    {
        float tempTimer = circles[activeHit].Timer;
        circles[activeHit].IsActive = false;

        activeHit = type;
        circles[activeHit].IsActive = true;
        circles[activeHit].Timer = tempTimer;
    }
}
