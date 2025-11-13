using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct ComputeBoidDirection : IJobEntity
{
    public NativeArray<LocalTransform> nearbyBoids;
    public float DeltaTime;

    float3 GetSeparationVector(LocalTransform transform, LocalTransform target)
    {
        float3 diff = transform.Position - target.Position;
        float diffLen = math.length(diff);
        float scaler = math.clamp(1.0f - diffLen / 2.0f, 0f, 1f);
        return diff * (scaler / diffLen);
    }

    public void Execute(ref BoidData data, ref LocalTransform transform)
    {
        quaternion currentRotation = transform.Rotation;
        float3 currentPosition = transform.Position;

        float3 separation = float3.zero;
        float3 alignment = transform.Up();
        float3 cohesion = transform.Position;
        int nearbors = 0;

        foreach (LocalTransform boid in nearbyBoids)
        {
            if (math.distance(currentPosition, boid.Position) >= 5.0f)
                continue;

            nearbors++;
            separation += GetSeparationVector(transform, boid);
            alignment += boid.Up();
            cohesion += boid.Position;
        }

        float average = 1.0f / nearbors;
        alignment *= average;
        cohesion *= average;
        cohesion = math.normalize(cohesion - currentPosition);

        data.direction = separation + alignment + cohesion;

        quaternion targetRotation = quaternion.LookRotationSafe(math.forward(), math.normalize(data.direction));
        if (!math.all(transform.Rotation.value == targetRotation.value))
        {
            float ip = math.exp(-4.0f * DeltaTime);
            transform.Rotation = math.slerp(currentRotation, targetRotation, ip);
        }

        transform.Position = transform.Position + transform.Up() * (data.speed * DeltaTime);
    }
}
