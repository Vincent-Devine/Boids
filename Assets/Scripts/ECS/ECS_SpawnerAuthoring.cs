using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BoidSpawnerAuthoring : MonoBehaviour
{
    public GameObject RatPrefab;
    public int Count = 100;
    public float SpawnRadius = 10f;

    class Baker : Baker<BoidSpawnerAuthoring>
    {
        public override void Bake(BoidSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new BoidSpawner
            {
                Prefab = GetEntity(authoring.RatPrefab, TransformUsageFlags.Dynamic),
                Count = authoring.Count,
                SpawnRadius = authoring.SpawnRadius
            });
        }
    }
}

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct BoidSpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        foreach (var spawner in SystemAPI.Query<RefRO<BoidSpawner>>())
        {
            var prefab = spawner.ValueRO.Prefab;
            var count = spawner.ValueRO.Count;
            var radius = spawner.ValueRO.SpawnRadius;
            var random = new Unity.Mathematics.Random(1234);

            var instances = state.EntityManager.Instantiate(prefab, count, Unity.Collections.Allocator.Temp);

            foreach (var entity in instances)
            {
                // Random point in Circle (X, Y) -> Map to (X, Z)
                float2 randomCircle = random.NextFloat2Direction() * random.NextFloat(0, radius);

                var transform = SystemAPI.GetComponent<LocalTransform>(entity);
                // CHANGE: Map Y to Z. Keep Y (Height) at 0.
                transform.Position = new float3(randomCircle.x, 0, randomCircle.y);
                transform.Rotation = quaternion.identity; // Reset rotation
                state.EntityManager.SetComponentData(entity, transform);
            }
        }
    }
}