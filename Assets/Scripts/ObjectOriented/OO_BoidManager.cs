using UnityEngine;

public class OO_BoidManager : MonoBehaviour
{
    [SerializeField] private BoidSettings boidSettings;
    private OO_Boid[] boids;
    
    void Start()
    {
        boids = FindObjectsByType<OO_Boid>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (boids == null || boids.Length == 0)
            return;

        foreach (OO_Boid boidA in boids)
        {
            boidA.avgFlockHeading = Vector3.zero;
            boidA.centerOffFlockmates = boidA.position;
            boidA.avgAvoidanceHeading = Vector3.zero;
            boidA.numPerceivedFlockmates = 0;

            foreach (OO_Boid boidB in boids)
            {
                if (boidA == boidB)
                    continue;
                
                Vector3 offset = boidB.position - boidA.position;
                float sqrDistance = offset.sqrMagnitude;
                
                if (sqrDistance < boidSettings.perceptionRadius * boidSettings.perceptionRadius)
                {
                    boidA.numPerceivedFlockmates++;
                    boidA.avgFlockHeading += boidB.forward;
                    boidA.centerOffFlockmates += boidB.position;

                    if (sqrDistance < boidSettings.avoidanceRadius * boidSettings.avoidanceRadius)
                        boidA.avgAvoidanceHeading -= offset / sqrDistance;
                }
                                    
                boidA.avgFlockHeading.y = 0.0f;
                boidA.centerOffFlockmates.y = 0.0f;
                boidA.avgAvoidanceHeading.y = 0.0f;
            }

            boidA.ManualUpdate();
        }
    }
}
