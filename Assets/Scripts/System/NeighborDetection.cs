using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

partial struct NeighborDetection : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>())
        {

        }

    }
}
