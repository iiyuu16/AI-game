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

    public EnemyState DefaultState;
    private EnemyState _state;
    public float IdleLocationRadius = 4f;
    public float IdleMovespeedMultiplier = 0.5f;

    public EnemyState State
    {
        get
        {
            return _state;
        }
        set
        {
            OnStateChange?.Invoke(_state, value);
            _state = value;
        }
    }

    public delegate void StateChangeEvent(EnemyState oldState, EnemyState newState);
    public StateChangeEvent OnStateChange;

    private void OnDisable()
    {
        _state = DefaultState;
    }

    private void HandleStateChange(EnemyState oldState, EnemyState newState)
    {
        if (oldState != newState)
        {
            if (followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
            }

            switch (newState)
            {
                case EnemyState.Idle:
                    break;
                case EnemyState.Patrol:
                    break;
                case EnemyState.Chase:
                    followCoroutine = StartCoroutine(followTarget());
                    break;
            }
        }
    }

    private IEnumerator DoIdleMotion()
    { 
        WaitForSeconds wait = new WaitForSeconds(updateSpeed);
        agent.speed *= IdleMovespeedMultiplier;

        while (true)
        {
            if (!agent.enabled || !agent.isOnNavMesh)
            {
                yield return wait;
            }
            else if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector2 point = Random.insideUnitCircle * IdleLocationRadius;
                NavMeshHit hit;

                if (NavMesh.SamplePosition(agent.transform.position + new Vector3(point.x, 0, point.y), out hit, 2f, agent.areaMask))
                {
                    agent.SetDestination(hit.position);
                }
            }
            yield return wait;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(followTarget());
    }

    private IEnumerator followTarget()
    {
        WaitForSeconds wait = new WaitForSeconds(updateSpeed);
        while (enabled) 
        {
            agent.SetDestination(target.transform.position);
            yield return null;
        }
    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);

            animator.SetBool(isRunning, agent.velocity.magnitude > 0.01f);

            if (agent.isOnOffMeshLink)
            {
                animator.SetTrigger(Jump);
            }
        }
        else
        {
            Debug.LogError("Player reference not set on EnemyController.");
        }
    }
}
