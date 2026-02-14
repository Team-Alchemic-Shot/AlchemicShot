using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PatrolFollow : MonoBehaviour
{
    public GameObject[] waypoint;
    int rand;
    GameObject waypointSelected;
    UnityEngine.AI.NavMeshAgent agent;
    public Transform target;
    public float minFollowRange;
    public float maxFollowRange;
    public bool following;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        following = false;
        Seed();
    }

    void Seed()
    {
        // waypoint = GameObject.FindGameObjectsWithTag("waypoint");
        if (waypoint.Length > 0)
        {
            rand = Random.Range(0, waypoint.Length);
            waypointSelected = waypoint[rand];
        }
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(agent.transform.position, target.transform.position);
        if (dist < maxFollowRange && dist >= minFollowRange)
        {
            agent.SetDestination(target.transform.position);
            Seed();
            following = true;
        }
        else
        {
           following = false;
            if (waypoint.Length > 0)
            {

                if (Vector3.Distance(agent.transform.position, waypointSelected.transform.position) >= 2)
                {
                    // pursue 'waypointSelected'
                    agent.SetDestination(waypointSelected.transform.position);
                }

                else
                {
                    // if the distance is too small, find a new 'waypointSelected'
                    Seed();
                }
            }
        }
    }
    
   /* void OnCollisionEnter (Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Scene thisScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(thisScene.name);
        }
    }*/
}
