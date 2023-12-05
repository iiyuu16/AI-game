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

    public EnemyState DefaultState;
    private EnemyState _state;
    public float IdleLocationRadius = 4f;
    public float IdleMovespeedMultiplier = 0.5f;
    private int WaypointIndex = 0;
    public Vector3[] waypoints = new Vector3[4];
    public NavMeshTriangulation triangulation;
    public EnemyLineOfSightChecker lineOfSightChecker;
    public EnemyScriptableObject EnemyScriptableObject;

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
        _state = EnemyScriptableObject.DefaultState;
    }

    private void HandleStateChange(EnemyState oldState, EnemyState newState)
    {
        if (oldState != newState)
        {
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
            if(!agent.enabled || !agent.isOnNavMesh)
            {
                yield return wait;
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
                waypoints[i] = hit.position;
            }
            else
            {
                Debug.LogError("Unable to find position for navmesh near triangulation vertex");
            }
        }

        OnStateChange?.Invoke(EnemyState.Spawn, EnemyScriptableObject.DefaultState);
    }

    private IEnumerator DoPatrolMotion()
    {
        WaitForSeconds wait = new WaitForSeconds(updateSpeed);

        yield return new WaitUntil(() => agent.enabled && agent.isOnNavMesh);
        agent.SetDestination(waypoints[WaypointIndex]);

        while (true)
        {
            if (agent.isOnNavMesh && agent.enabled && agent.remainingDistance <= agent.stoppingDistance)
            {
                WaypointIndex++;

                if (WaypointIndex >= waypoints.Length)
                {
                    WaypointIndex = 0;
                }
                agent.SetDestination(waypoints[WaypointIndex]);
            }
            yield return wait;
        }
    }



    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        lineOfSightChecker.onGainSight += HandleGainSight;
        lineOfSightChecker.onLoseSight += HandleLoseSight;

        OnStateChange += HandleStateChange;
    }

    private void HandleGainSight(Player player)
    {
        State = EnemyState.Chase;
    }

    private void HandleLoseSight(Player player)
    {
        State = EnemyScriptableObject.DefaultState;
    }

    private void Start()
    {
        State = EnemyScriptableObject.DefaultState;
        OnStateChange += HandleStateChange;
        
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
}