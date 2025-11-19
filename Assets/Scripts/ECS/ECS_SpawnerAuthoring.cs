using Unity.Entities;
using UnityEngine;

public class ECS_SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public float InitialRadius;
    public int Count;

    class Baker : Baker<ECS_SpawnerAuthoring>
    {
        public override void Bake(ECS_SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new ECS_Spawner
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Renderable|TransformUsageFlags.WorldSpace),
                Count = authoring.Count,
                InitialRadius = authoring.InitialRadius
            });
        }
    }
}

public struct ECS_Spawner : IComponentData
{
    public Entity Prefab;
    public float InitialRadius;
    public int Count;
}