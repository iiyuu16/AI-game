using System.Collections;
using Unity.VisualScripting;
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

    public EnemyState DefaultState = EnemyState.Spawn;
    private EnemyState _state;
    public float IdleLocationRadius = 4f;
    public float IdleMovespeedMultiplier = 0.5f;
    private int WaypointIndex = 0;
    //public Vector3[] waypoints = new Vector3[4];
    public Transform[] waypoints = new Transform[4];
    public NavMeshTriangulation triangulation;
    public EnemyLineOfSightChecker lineOfSightChecker;

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
    private Coroutine followCoroutine;

    private void OnDisable()
    {
        _state = DefaultState;
    }

    private void HandleStateChange(EnemyState oldState, EnemyState newState)
    {
        if (oldState != newState)
        {
            if(oldState == EnemyState.Idle)
            {
                agent.speed /= IdleMovespeedMultiplier;
            }

            if(followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
            }
            
            switch (newState)
            {
                case EnemyState.Idle:
                    Debug.Log("entering idle state");
                    followCoroutine = StartCoroutine(DoIdleMotion());
                    break;
                case EnemyState.Patrol:
                    Debug.Log("entering patrol state");
                    followCoroutine = StartCoroutine(DoPatrolMotion());
                    break;
                case EnemyState.Chase:
                    Debug.Log("entering chase state");
                    followCoroutine = StartCoroutine(followTarget());
                    break;
            }
        }
    }

    private IEnumerator DoIdleMotion()
    {
        WaitForSeconds wait = new WaitForSeconds(updateSpeed);

        agent.speed += IdleMovespeedMultiplier;

        while (true)
        {
            if(agent.enabled || !agent.isOnNavMesh)
            {
                yield return null;
            }
            else if(agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector2 point = Random.insideUnitCircle * IdleLocationRadius;
                NavMeshHit hit;

                if(NavMesh.SamplePosition(agent.transform.position + new Vector3(point.x, 0, point.y), out hit, 2f, agent.areaMask))
                {
                    agent.SetDestination(hit.position);
                }
            }
            yield return wait;
        }
    }

    public void Spawn()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(triangulation.vertices[Random.Range(0, triangulation.vertices.Length)], out hit, 2f, agent.areaMask))
            {
                waypoints[i].position = hit.position;
            }
            else
            {
                Debug.LogError("Unable to find position for navmesh near triangulation vertex");
            }
        }

        OnStateChange?.Invoke(EnemyState.Spawn, DefaultState);

        // Debug statements
        Debug.Log("Spawn method executed successfully.");
        foreach (Transform waypoint in waypoints)
        {
            Debug.Log("Waypoint position: " + waypoint.position);
        }
    }

    private IEnumerator DoPatrolMotion()
    {
        WaitForSeconds wait = new WaitForSeconds(updateSpeed);

        yield return new WaitUntil(() => agent.enabled && agent.isOnNavMesh);

        while (true)
        {
            if (agent.isOnNavMesh && agent.enabled && agent.remainingDistance <= agent.stoppingDistance)
            {
                target = waypoints[WaypointIndex];

                agent.SetDestination(target.position);

                yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);

                DoIdleMotion();
                yield return new WaitForSeconds(3f);

                WaypointIndex++;
                if (WaypointIndex >= waypoints.Length)
                {
                    WaypointIndex = 0;
                }

                target = waypoints[WaypointIndex];

                State = EnemyState.Patrol;

                agent.SetDestination(target.position);
            }
            yield return wait;
        }
    }



    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        lineOfSightChecker.onGainSight += HandleGainSight;
        lineOfSightChecker.onLoseSight += HandleLoseSight;
    }

    private void HandleGainSight(Player player)
    {
        State = EnemyState.Chase;
    }

    private void HandleLoseSight(Player player)
    {
        State = DefaultState;
    }

    private void Start()
    {
        StartCoroutine(followTarget());

        OnStateChange += HandleStateChange;

        State = DefaultState;
    }

    private IEnumerator followTarget() //chase
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