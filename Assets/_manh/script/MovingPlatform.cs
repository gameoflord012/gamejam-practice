using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    [SerializeField] Transform transformA;
    [SerializeField] Transform transformB;

    [SerializeField] float travelDuration = 1;

    int currentIndex;
    float travelTimer;
    Vector2[] positions = new Vector2[2];

    private void Start()
    {
        currentIndex = 0;
        travelTimer = 0;
        positions[0] = transformA.position;
        positions[1] = transformB.position;
    }

    private void Update()
    {
        if(travelTimer > travelDuration)
        {
            travelTimer = 0;
            currentIndex = (currentIndex + 1) % 2;
        }

        Vector2 nextWaypoint = positions[(currentIndex + 1) % 2];

        rb.velocity = (nextWaypoint - positions[currentIndex]) / travelDuration; 

        travelTimer += Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transformA.position, 0.3f);
        Gizmos.DrawSphere(transformB.position, 0.3f);

        Gizmos.DrawLine(transform.position, transformA.position);
        Gizmos.DrawLine(transformA.position, transformB.position);
    }
}
