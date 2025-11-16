using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject boidParent;
    [SerializeField] private BoidSettings boidSettings;
    [SerializeField] private int spawnNumber = 50;
    [SerializeField] private float spawnRadius = 10.0f;

    void Awake()
    {
        for (int i = 0; i < spawnNumber; i++)
        {
            Vector3 position = transform.position + new Vector3(Random.Range(0.0f, spawnRadius), 0.0f, Random.Range(0.0f, spawnRadius));
            OO_Boid boid = Instantiate(prefab, position, Random.rotation).GetComponent<OO_Boid>();
            boid.transform.parent = boidParent.transform;
            boid.Initialize(boidSettings);
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo();
    }

    private void OnDrawGizmos()
    {
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
