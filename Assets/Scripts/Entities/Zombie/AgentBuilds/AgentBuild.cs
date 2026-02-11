using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBuild : MonoBehaviour
{
    // agent seed modifier
    public float speed = 5f;

    // agent's direction of movement
    protected Vector3 direction;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        direction = Random.insideUnitSphere;
        direction.y = 0f; // keep the agent's movement on XZ plane
        direction.Normalize();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        AvoidObstacle(speed); // moving with obstacle check
    }

    // when moving, avoids obstacles using raycasting and bounce logic
    protected void AvoidObstacle(float movementSpeed)
    {
        // Lift ray cast slightly to avoid ground collision
        Vector3 origin = transform.position + new Vector3(0f, 0.3f, 0f);
        Vector3 dirc = direction.normalized;
        float rayDistance = 0.8f;

        // debug line
        Debug.DrawRay(origin, dirc * rayDistance, Color.red);

        RaycastHit hit;
        if (!Physics.Raycast(origin, dirc, out hit, rayDistance))
        {
            // move forward when no obsticles
            transform.position += dirc * movementSpeed * Time.deltaTime;
        }
        else
        {
            // bounce of any obsticle when hitting one
            /*Vector3 reflected = Vector3.Reflect(dirc, hit.normal);
            reflected = Quaternion.Euler(0, Random.Range(-30f, 30f), 0) * reflected;
            reflected.y = 0f;
            reflected.Normalize();

            direction = reflected;*/

            // helps to stop sticking to walls
            transform.position += direction * movementSpeed * Time.deltaTime * 0.2f;
        }
    }
}
