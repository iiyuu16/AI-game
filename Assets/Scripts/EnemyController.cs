using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [SerializeField]
    public Transform target;
    public float updateSpeed;
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Animator animator;

    private const string isRunning = "isRunning";
    private const string Jump = "Jump";

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            // Set the player's position as the destination
            agent.SetDestination(target.position);

            // Check if the agent is running
            animator.SetBool(isRunning, agent.velocity.magnitude > 0.01f);

            // Check if the agent is on an off-mesh link
            if (agent.isOnOffMeshLink)
            {
                // Trigger the Jump animation
                animator.SetTrigger(Jump);
            }
        }
        else
        {
            Debug.LogError("Player reference not set on EnemyController.");
        }
    }
}
