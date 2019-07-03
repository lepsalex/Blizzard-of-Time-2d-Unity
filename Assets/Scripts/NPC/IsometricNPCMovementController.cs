using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class IsometricNPCMovementController : MonoBehaviour
{

    // this is temp for a* demo only
    public Transform target;

    public float movementSpeed = 1f;
    public float nextWaypointDistance = 6f;
    public float endDistance = 1f;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndofPath = false;
    float moveForceMultiplier = 100f;

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
        // this is temp for a* demo only
        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (path == null)
            return;
            
        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndofPath = true;
            return;
        }
        else
        {
            reachedEndofPath = false;
        }

        Vector2 direction = Vector2.ClampMagnitude(((Vector2)path.vectorPath[currentWaypoint] - rbody.position), 1).normalized;
        Vector2 force = direction * (movementSpeed * moveForceMultiplier) * Time.fixedDeltaTime;

        isoRenderer.SetDirection(force);
        rbody.AddForce(force);

        float distance = Vector2.Distance(rbody.position, path.vectorPath[currentWaypoint]);

        if (distance <= nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    void UpdatePath()
    {
        float distanceToEnd = Vector2.Distance(rbody.position, target.transform.position);

        if (seeker.IsDone() && distanceToEnd > endDistance) {
            seeker.StartPath(rbody.position, target.position, OnPathComplete);
            // seeker.RunModifiers() (TODO)
        }
            
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}
