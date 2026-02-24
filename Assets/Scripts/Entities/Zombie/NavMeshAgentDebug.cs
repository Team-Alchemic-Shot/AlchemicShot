using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentDebug : MonoBehaviour
{
    [SerializeField] private bool logEverySecond = false;
    [SerializeField] private float interval = 1f;

    private NavMeshAgent agent;
    private float timer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent == null) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            LogOnce();
        }

        if (!logEverySecond) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = interval;
            LogOnce();
        }
    }

    private void LogOnce()
    {
        Debug.Log(
            $"{name} | onNavMesh={agent.isOnNavMesh} | stopped={agent.isStopped} | hasPath={agent.hasPath} | " +
            $"status={agent.pathStatus} | remaining={agent.remainingDistance:F2} | vel={agent.velocity.magnitude:F2} | " +
            $"dest={agent.destination}"
        );
    }
}