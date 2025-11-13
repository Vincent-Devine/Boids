using Unity.Entities;
using Unity.Mathematics;

public struct BoidData : IComponentData
{
    public float3 direction;
    public float speed;
}