using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorAgent : AgentBuild
{
    // the distance the predator can see any prey
    public float visualRadius = 6f;

    // // the visual cone angle they can see in degrees
    public float visualAngle = 90f;

    // the speed the predator can chase prey
    public float chaseSpeed = 6f;

    // the current target prey the predator is chasing
    private GameObject targetPrey;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        targetPrey = FindNearestPrey();

        if (targetPrey != null)
        {
            // chasing prey target
            direction = (targetPrey.transform.position - transform.position).normalized;
            AvoidObstacle(chaseSpeed);
        }
        else
        {
            AvoidObstacle(speed); // move forward while avoiding obstacles
        }

        transform.forward = direction;
    }

    // finds the nearest prey within the predator's cone of vision and target them
    GameObject FindNearestPrey()
    {
        GameObject[] preyList = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float minDist = visualRadius;

        foreach (GameObject prey in preyList)
        {
            Vector3 toPrey = prey.transform.position - transform.position;
            float dist = toPrey.magnitude;

            // check borh distance and angle, to check if prey is within sight
            if (dist < visualRadius)
            {
                float angle = Vector3.Angle(transform.forward, toPrey.normalized);

                if (angle < visualAngle / 2f)
                {
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = prey;
                    }
                }
            }
        }

        return closest;
    }
}
