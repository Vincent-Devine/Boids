using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
public partial struct BoidSystem : ISystem
{
    private NativeArray<float> _checkAngles;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidConfig>();
        state.RequireForUpdate<PhysicsWorldSingleton>();

        _checkAngles = new NativeArray<float>(new float[]
        {
            math.radians(15), math.radians(-15),
            math.radians(35), math.radians(-35),
            math.radians(60), math.radians(-60),
            math.radians(90), math.radians(-90)
        }, Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_checkAngles.IsCreated) _checkAngles.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<BoidConfig>();
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var boidQuery = SystemAPI.QueryBuilder().WithAll<BoidTag, LocalTransform, BoidVelocity>().Build();
        if (boidQuery.CalculateEntityCount() == 0) return;

        var allPositions = boidQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var allVelocities = boidQuery.ToComponentDataArray<BoidVelocity>(Allocator.TempJob);

        var job = new BoidJob
        {
            AllPositions = allPositions,
            AllVelocities = allVelocities,
            CheckAngles = _checkAngles,
            Config = config,
            DeltaTime = SystemAPI.Time.DeltaTime,
            CollisionWorld = physicsWorld.CollisionWorld
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct BoidJob : IJobEntity
{
    [ReadOnly][DeallocateOnJobCompletion] public NativeArray<LocalTransform> AllPositions;
    [ReadOnly][DeallocateOnJobCompletion] public NativeArray<BoidVelocity> AllVelocities;
    [ReadOnly] public NativeArray<float> CheckAngles;
    [ReadOnly] public CollisionWorld CollisionWorld;

    public BoidConfig Config;
    public float DeltaTime;

    private void Execute(Entity entity, [EntityIndexInQuery] int index, ref LocalTransform transform, ref BoidVelocity velocity)
    {
        float2 currentPos = transform.Position.xz;
        float2 currentVel = velocity.Value;

        float speed = math.length(currentVel);
        float2 forward = (speed > 0.001f) ? (currentVel / speed) : new float2(1, 0);

        float2 separationSum = float2.zero;
        float2 alignmentSum = float2.zero;
        float2 cohesionSum = float2.zero;
        int neighborCount = 0;

        for (int i = 0; i < AllPositions.Length; i++)
        {
            if (i == index) continue;

            float2 otherPos = AllPositions[i].Position.xz;
            float distSq = math.distancesq(currentPos, otherPos);

            if (distSq < Config.NeighborDistance * Config.NeighborDistance)
            {
                float distance = math.sqrt(distSq);
                if (distance < Config.SeparationDistance)
                {
                    float push = (Config.SeparationDistance - distance) / Config.SeparationDistance;
                    separationSum += (currentPos - otherPos) * push;
                }
                alignmentSum += AllVelocities[i].Value;
                cohesionSum += otherPos;
                neighborCount++;
            }
        }

        float2 flockingForce = float2.zero;
        if (neighborCount > 0)
        {
            alignmentSum /= neighborCount;
            cohesionSum /= neighborCount;
            cohesionSum -= currentPos;

            flockingForce += separationSum * Config.SeparationWeight;
            flockingForce += math.normalizesafe(alignmentSum) * Config.AlignmentWeight;
            flockingForce += math.normalizesafe(cohesionSum) * Config.CohesionWeight;
        }

        float2 avoidanceForce = float2.zero;
        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = Config.ObstacleLayerMask,
            GroupIndex = 0
        };

        if (CastRay(currentPos, forward, Config.ObstacleViewDistance, filter))
        {
            float2 bestDir = forward;
            bool foundPath = false;

            for (int i = 0; i < CheckAngles.Length; i++)
            {
                float angle = CheckAngles[i];
                float cos = math.cos(angle);
                float sin = math.sin(angle);

                float2 scanDir = new float2(
                    forward.x * cos - forward.y * sin,
                    forward.x * sin + forward.y * cos
                );

                if (!CastRay(currentPos, scanDir, Config.ObstacleViewDistance, filter))
                {
                    bestDir = scanDir;
                    foundPath = true;
                    break;
                }
            }

            if (foundPath)
            {
                float2 targetVel = bestDir * Config.MoveSpeed;
                avoidanceForce = (targetVel - currentVel) * Config.ObstacleWeight;
            }
            else
            {
                avoidanceForce = -currentVel * Config.ObstacleWeight * 2f;
            }
        }

        float2 acceleration = flockingForce + avoidanceForce;
        currentVel += acceleration * DeltaTime;

        float newSpeed = math.length(currentVel);
        if (newSpeed > Config.MoveSpeed)
        {
            currentVel = (currentVel / newSpeed) * Config.MoveSpeed;
        }
        if (newSpeed < Config.MoveSpeed * 0.5f)
        {
            currentVel = math.normalizesafe(currentVel, new float2(1, 0)) * Config.MoveSpeed * 0.5f;
        }

        float2 nextPos = currentPos + (currentVel * DeltaTime);

        if (nextPos.x > Config.BoundsMax.x) { nextPos.x = Config.BoundsMax.x; currentVel.x *= -1; }
        if (nextPos.x < Config.BoundsMin.x) { nextPos.x = Config.BoundsMin.x; currentVel.x *= -1; }
        if (nextPos.y > Config.BoundsMax.y) { nextPos.y = Config.BoundsMax.y; currentVel.y *= -1; }
        if (nextPos.y < Config.BoundsMin.y) { nextPos.y = Config.BoundsMin.y; currentVel.y *= -1; }

        velocity.Value = currentVel;
        transform.Position = new float3(nextPos.x, 0.1f, nextPos.y);

        if (math.lengthsq(currentVel) > 0.01f)
        {
            float angle = math.atan2(currentVel.y, currentVel.x);
            transform.Rotation = quaternion.AxisAngle(math.up(), -angle);
        }
    }

    private bool CastRay(float2 start, float2 dir, float dist, CollisionFilter filter)
    {
        RaycastInput input = new RaycastInput
        {
            Start = new float3(start.x, 1.0f, start.y),
            End = new float3(start.x + dir.x * dist, 1.0f, start.y + dir.y * dist),
            Filter = filter
        };
        return CollisionWorld.CastRay(input);
    }
}