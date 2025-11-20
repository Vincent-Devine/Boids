using Unity.Entities;
using Unity.Mathematics;

public struct BoidTag : IComponentData
{
}

public struct BoidVelocity : IComponentData
{
    public float2 Value;
}

public struct BoidConfig : IComponentData
{
    public float MoveSpeed;

    public float NeighborDistance;
    public float SeparationDistance;
    public float SeparationWeight;
    public float AlignmentWeight;
    public float CohesionWeight;

    public float ObstacleViewDistance;
    public float ObstacleWeight;
    public uint ObstacleLayerMask;

    public float2 BoundsMin;
    public float2 BoundsMax;
}

public struct BoidSpawner : IComponentData
{
    public Entity Prefab;
    public int Count;
    public float SpawnRadius;
}