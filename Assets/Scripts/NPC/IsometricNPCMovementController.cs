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
    bool endOfPathReached = false;
    float moveSpeed = 1f;

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
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            endOfPathReached = true;
            return;
        }
        else
        {
            endOfPathReached = false;
        }

        DisableMovementIfNearTarget();

        Vector2 nextPosition = (Vector2)path.vectorPath[currentWaypoint];
        Vector2 direction = Vector2.ClampMagnitude((nextPosition - rbody.position), 1).normalized;

        isoRenderer.SetDirection(direction);
        transform.position = Vector2.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

        float distance = Vector2.Distance(rbody.position, path.vectorPath[currentWaypoint]);

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
            if (endOfPathReached && stopOnPathCompletion)
                yield break;

            float distanceToTarget = Vector2.Distance(rbody.position, moveTarget.transform.position);

            if (seeker.IsDone() && distanceToTarget > endDistance)
            {
                seeker.StartPath(rbody.position, moveTarget.position, OnPathCalculationComplete);
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
    private void DisableMovementIfNearTarget()
    {
        float distanceToTarget = Vector2.Distance(rbody.position, moveTarget.transform.position);

        if (distanceToTarget < endDistance)
        {
            rbody.isKinematic = true;
            return;
        }
        else
        {
            rbody.isKinematic = false;
        }
    }
    private void Reset()
    {
        StopAllCoroutines();
        endOfPathReached = false;
    }
}
