using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

partial struct BoidMovement : ISystem
{
    private EntityQuery query;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        query = state.GetEntityQuery(ComponentType.ReadOnly<LocalTransform>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NativeArray<LocalTransform> boidTransforms = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        ComputeBoidDirection computeBoidDirection = new ComputeBoidDirection
        {
            nearbyBoids = boidTransforms,
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        computeBoidDirection.ScheduleParallel();
    }
}
