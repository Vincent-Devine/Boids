using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine; // Required for Debug.DrawLine

[BurstCompile]
public partial struct BoidSystem : ISystem
{
    private NativeArray<float> _checkAngles;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidConfig>();
        state.RequireForUpdate<PhysicsWorldSingleton>();

        // Angles: +/- 15, 35, 60, 90 degrees
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

    // [BurstCompile] -> Comment out Burst temporarily if you want to see Debug.DrawLine errors in Console
    // But for performance, keep it. Debug.DrawLine works in OnUpdate without Burst usually, 
    // but strictly speaking, we can't DrawLine inside a Burst Job. 
    // We will only draw lines in the OnUpdate (Main Thread) for a few boids to test.
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<BoidConfig>();
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var boidQuery = SystemAPI.QueryBuilder().WithAll<BoidTag, LocalTransform, BoidVelocity>().Build();
        if (boidQuery.CalculateEntityCount() == 0) return;

        var allPositions = boidQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var allVelocities = boidQuery.ToComponentDataArray<BoidVelocity>(Allocator.TempJob);

        // --- DEBUG DRAWING (Main Thread - Slow, but good for testing) ---
        // Uncomment this block to visualize what is happening!
        /*
        int debugCount = 0;
        foreach (var (transform, velocity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<BoidVelocity>>())
        {
            if (debugCount > 10) break; // Only draw for 10 rats
            float3 pos = transform.ValueRO.Position;
            float3 vel = new float3(velocity.ValueRO.Value.x, 0, velocity.ValueRO.Value.y);
            
            // Draw Direction (Green)
            Debug.DrawLine(pos, pos + math.normalize(vel) * 2f, Color.green);
            
            // Draw Raycast (Red)
            float3 rayEnd = pos + math.normalize(vel) * config.ObstacleViewDistance;
            Debug.DrawLine(pos, rayEnd, Color.red);
            
            debugCount++;
        }
        */
        // ---------------------------------------------------------------

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
        float2 currentPos = transform.Position.xz; // X and Z
        float2 currentVel = velocity.Value; // X and Z

        // Prevent divide by zero
        float speed = math.length(currentVel);
        float2 forward = (speed > 0.001f) ? (currentVel / speed) : new float2(1, 0);

        // --- 1. FLOCKING LOGIC ---
        float2 separationSum = float2.zero;
        float2 alignmentSum = float2.zero;
        float2 cohesionSum = float2.zero;
        int neighborCount = 0;

        // Safety: Limit checking to prevent crashing if too many boids (optimization later)
        // For now, brute force is okay for < 1000 boids
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
                    // Strong separation to prevent stacking
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

            flockingForce += separationSum * Config.SeparationWeight; // Removed normalize for stronger push
            flockingForce += math.normalizesafe(alignmentSum) * Config.AlignmentWeight;
            flockingForce += math.normalizesafe(cohesionSum) * Config.CohesionWeight;
        }

        // --- 2. OBSTACLE AVOIDANCE ---
        float2 avoidanceForce = float2.zero;

        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = Config.ObstacleLayerMask,
            GroupIndex = 0
        };

        if (CastRay(currentPos, forward, Config.ObstacleViewDistance, filter))
        {
            // Path blocked
            float2 bestDir = forward; // Default
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

            // Apply Force
            if (foundPath)
            {
                // Turn towards freedom
                float2 targetVel = bestDir * Config.MoveSpeed;
                avoidanceForce = (targetVel - currentVel) * Config.ObstacleWeight;
            }
            else
            {
                // Turn around completely
                avoidanceForce = -currentVel * Config.ObstacleWeight * 2f;
            }
        }

        // --- 3. INTEGRATION ---
        // Avoidance has priority
        float2 acceleration = flockingForce + avoidanceForce;

        currentVel += acceleration * DeltaTime;

        // Speed Clamp
        float newSpeed = math.length(currentVel);
        if (newSpeed > Config.MoveSpeed)
        {
            currentVel = (currentVel / newSpeed) * Config.MoveSpeed;
        }
        // Optional: Min Speed to keep them moving
        if (newSpeed < Config.MoveSpeed * 0.5f)
        {
            currentVel = math.normalizesafe(currentVel, new float2(1, 0)) * Config.MoveSpeed * 0.5f;
        }

        // --- 4. BOUNDS (BOUNCE MODE) ---
        // This is easier to debug than wrapping
        float2 nextPos = currentPos + (currentVel * DeltaTime);

        if (nextPos.x > Config.BoundsMax.x) { nextPos.x = Config.BoundsMax.x; currentVel.x *= -1; }
        if (nextPos.x < Config.BoundsMin.x) { nextPos.x = Config.BoundsMin.x; currentVel.x *= -1; }

        // BoundsMax.y acts as Z limit
        if (nextPos.y > Config.BoundsMax.y) { nextPos.y = Config.BoundsMax.y; currentVel.y *= -1; }
        if (nextPos.y < Config.BoundsMin.y) { nextPos.y = Config.BoundsMin.y; currentVel.y *= -1; }

        // --- APPLY ---
        velocity.Value = currentVel;
        transform.Position = new float3(nextPos.x, 0, nextPos.y);

        // --- ROTATION FIX ---
        // Only rotate if we are actually moving
        if (math.lengthsq(currentVel) > 0.01f)
        {
            // atan2(y, x) using our Z as Y
            float angle = math.atan2(currentVel.y, currentVel.x);
            transform.Rotation = quaternion.AxisAngle(math.up(), -angle);
            // Note: Try 'angle' or '-angle'. If they rotate opposite to turn, flip the sign.
            // Since 3D X/Z usually has Z as Forward, but Math uses X as Right (0 rad), 
            // -angle is often required to map standard Unit Circle math to Unity's Compass.
        }
    }

    private bool CastRay(float2 start, float2 dir, float dist, CollisionFilter filter)
    {
        // IMPORTANT: We lift the ray up (Y=1.0f) to ensure it hits walls that are sitting on Y=0
        RaycastInput input = new RaycastInput
        {
            Start = new float3(start.x, 1.0f, start.y),
            End = new float3(start.x + dir.x * dist, 1.0f, start.y + dir.y * dist),
            Filter = filter
        };
        return CollisionWorld.CastRay(input);
    }
}