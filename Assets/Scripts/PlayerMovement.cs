using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;
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
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                agent.SetDestination(hit.point);
            }
        }

        animator.SetBool(isRunning, agent.velocity.magnitude > 0.01f);

        // Check if the agent is on an off-mesh link
        if (agent.isOnOffMeshLink)
        {
            // Trigger the Jump animation
            animator.SetTrigger(Jump);
        }
    }
}
