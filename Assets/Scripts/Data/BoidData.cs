using Unity.Entities;
using Unity.Mathematics;

public struct DirectionData : IComponentData
{
    public float3 direction;
}

public struct SpeedData : IComponentData
{
    public float speed;
}

public struct NeighborsEntity : IBufferElementData
{
    public Entity neighbors;
}