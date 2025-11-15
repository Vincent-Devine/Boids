using UnityEngine;

[CreateAssetMenu(fileName = "BoidSettings", menuName = "Scriptable Objects/BoidSettings")]
public class BoidSettings : ScriptableObject
{
    [Header("Boids")]
    public float minSpeed = 2.0f;
    public float maxSpeed = 5.0f;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1.0f;
    public float maxSteerForce = 3.0f;

    [Header("Weight")]
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float separationWeight = 1.0f;
    
    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float boundsRadius = 0.27f;
    public float avoidCollisionWeight = 10.0f;
    public float collisionAvoidDist = 5.0f;
}