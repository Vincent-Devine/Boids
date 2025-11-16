using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class OO_Boid : MonoBehaviour
{
    private BoidSettings boidSettings;

    private static readonly Vector3[] rayDirections2D =
    {
        new Vector3( 1.0f, 0.0f,  0.0f),
        new Vector3(-1.0f, 0.0f,  0.0f),
        new Vector3( 0.0f, 0.0f,  1.0f),
        new Vector3( 0.0f, 0.0f, -1.0f),
        new Vector3( 1.0f, 0.0f,  1.0f).normalized,
        new Vector3( 1.0f, 0.0f, -1.0f).normalized,
        new Vector3(-1.0f, 0.0f, -1.0f).normalized,
        new Vector3(-1.0f, 0.0f,  1.0f).normalized,
    };
    
    // State
    private Vector3 velocity;
    
    // Update
    private Vector3 acceleration;
    [HideInInspector] public Vector3 avgFlockHeading;
    [HideInInspector] public Vector3 avgAvoidanceHeading;
    [HideInInspector] public Vector3 centerOffFlockmates;
    [HideInInspector] public int numPerceivedFlockmates;
    
    // Cache
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Vector3 forward;

    public void Initialize(BoidSettings boidSettings)
    {
        this.boidSettings = boidSettings;
        
        position = transform.position;
        forward = transform.forward;
        float startSpeed = (boidSettings.minSpeed + boidSettings.maxSpeed) / 2;
        velocity = forward * startSpeed;
    }

    public void ManualUpdate()
    {
        Vector3 acceleration = Vector3.zero;

        if (numPerceivedFlockmates != 0)
        {
            centerOffFlockmates /= numPerceivedFlockmates;
            Vector3 offsetToFlockmatesCenter = centerOffFlockmates - position;
            
            Vector3 alignmentForce = SteerTowards(avgFlockHeading) * boidSettings.alignmentWeight;
            Vector3 cohesionForce = SteerTowards(offsetToFlockmatesCenter) * boidSettings.cohesionWeight;
            Vector3 separationForce = SteerTowards(avgAvoidanceHeading) * boidSettings.separationWeight;
            
            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += separationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * boidSettings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }
        
        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 direction = velocity / speed;
        speed = Mathf.Clamp(speed, boidSettings.minSpeed, boidSettings.maxSpeed);
        velocity = direction * speed;

        velocity.y = 0.0f;
        direction.y = 0.0f;
        
        transform.position += velocity * Time.deltaTime;
        transform.forward = direction;
        
        position =  transform.position;
        forward = transform.forward;
    }

    private Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * boidSettings.maxSpeed - velocity;
        v.y = 0.0f;
        return Vector3.ClampMagnitude(v, boidSettings.maxSteerForce);
    }
    
    private bool IsHeadingForCollision()
    {
        RaycastHit hit;
        return Physics.SphereCast(position, boidSettings.boundsRadius, forward, out hit, boidSettings.collisionAvoidDist, boidSettings.obstacleMask);
    }

    private Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = rayDirections2D;
        RaycastHit hit;

        foreach (Vector3 rayDirection in rayDirections)
        {
            Vector3 direction = transform.TransformDirection(rayDirection);
            if (!Physics.SphereCast(position, boidSettings.boundsRadius, direction, out hit, boidSettings.collisionAvoidDist, boidSettings.obstacleMask))
                return direction;
        }
        
        return forward;
    }
}
