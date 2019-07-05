using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(IsometricCharacterRenderer))]
[RequireComponent(typeof(SimpleSmoothModifier))]
public class IsometricNPCMovementController : MonoBehaviour
{

    // this is temp for a* demo only
    public Transform moveTarget { get; set; }

    public float movementSpeed = 1f;
    public float nextWaypointDistance = 6f;
    public float endDistance = 1f;

    Path path;
    int currentWaypoint = 0;
    bool targetReached = false;
    float distanceToTarget = 0f;

    Seeker seeker;
    Rigidbody2D rbody;
    IsometricCharacterRenderer isoRenderer;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        rbody = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
    }

    void Start()
    {
        // temp set player as target in start
        moveTarget = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void MoveToTarget()
    {
        Reset();
        StartCoroutine(UpdatePath(stopOnPathCompletion: true));
    }

    public void FollowTarget()
    {
        Reset();
        StartCoroutine(UpdatePath(stopOnPathCompletion: false));
    }

    // Update is called once per frame
    void Update()
    {
        DisableMovementIfAtTarget();

        if (path == null || targetReached)
            return;

        Vector2 nextPosition = (Vector2)path.vectorPath[currentWaypoint];
        Vector2 direction = Vector2.ClampMagnitude((nextPosition - (Vector2) transform.position), 1);

        isoRenderer.SetDirection(direction);
        transform.position = Vector2.MoveTowards(transform.position, nextPosition, movementSpeed * Time.deltaTime);

        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);

        if (distance <= nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }


    private IEnumerator UpdatePath(bool stopOnPathCompletion)
    {
        while (true)
        {
            // If set to stop moving on path complete, end coroutine here
            if (targetReached && stopOnPathCompletion)
                yield break;

            if (seeker.IsDone())
            {
                seeker.StartPath(transform.position, moveTarget.position, OnPathCalculationComplete);
            }

            yield return new WaitForSeconds(.5f);
        }
    }

    private void OnPathCalculationComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    /// Disabled body kinematics near target (avoids pushing)
    private void DisableMovementIfAtTarget()
    {
        distanceToTarget = Vector2.Distance(transform.position, moveTarget.transform.position);

        if (distanceToTarget > endDistance)
        {
            rbody.isKinematic = false;
            targetReached = false;
        }
        else
        {
            rbody.isKinematic = true;
            targetReached = true;
            path = null;
        }
    }
    private void Reset()
    {
        StopAllCoroutines();
        targetReached = false;
    }
}
