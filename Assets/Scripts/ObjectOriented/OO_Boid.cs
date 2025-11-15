using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class OO_Boid : MonoBehaviour
{
    private BoidSettings boidSettings;

    // State
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Vector3 forward;
    private Vector3 velocity;
    
    // Update
    private Vector3 acceleration;
    [HideInInspector] public Vector3 avgFlockHeading;
    [HideInInspector] public Vector3 avgAvoidanceHeading;
    [HideInInspector] public Vector3 centerOffFlockmates;
    [HideInInspector] public int numPerceivedFlockmates;

    public void Initialize(BoidSettings boidSettings)
    {
        this.boidSettings = boidSettings;
        position = transform.position;
        forward = transform.forward;
        
        float startSpeed = (boidSettings.minSpeed + boidSettings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
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
        
        transform.position += velocity * Time.deltaTime;
        transform.forward = direction;
        position = transform.position;
        forward = direction;
    }

    private Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * boidSettings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, boidSettings.maxSteerForce);
    }
    
    private bool IsHeadingForCollision()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, boidSettings.boundsRadius, transform.forward, out hit, boidSettings.collisionAvoidDist, boidSettings.obstacleMask);
    }

    private Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = GetRayDirections();
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 direction = transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(position, direction);
            if(!Physics.SphereCast(ray, boidSettings.boundsRadius, boidSettings.collisionAvoidDist, boidSettings.obstacleMask))
                return direction;
        }
        
        return forward;
    }
    
    private Vector3[] GetRayDirections()
    {
        const int NUM_VIEW_DIRECTION = 300;
        Vector3[] directions = new Vector3[NUM_VIEW_DIRECTION];

        const float GOLDEN_RATIO = 1.61803398875f;
        float angleIncrement = Mathf.PI * 2.0f * GOLDEN_RATIO;

        for (int i = 0; i < NUM_VIEW_DIRECTION; i++)
        {
            float t = (float)i / (float)NUM_VIEW_DIRECTION;
            float inclination = Mathf.Acos(1.0f - 2.0f * t);
            float azimuth = angleIncrement * i;
            
            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            directions[i] = new Vector3(x, y, z);
        }

        return directions;
    }
}
