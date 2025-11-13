using System.Diagnostics;
using System.Net.Http.Headers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct ComputeBoidDirection : IJobEntity
{
    public NativeArray<LocalTransform> nearbyBoids;
    public float DeltaTime;

    float3 GetSeparationVector(LocalTransform transform, LocalTransform target)
    {
        float3 diff = transform.Position - target.Position;
        float3 diffLen = math.length(diff);
        float3 scaler = math.clamp(1.0f - diffLen / 2.0f, 0, 1);
        return diff * (scaler / diffLen);
    }

    public void Execute(ref DirectionData data, ref LocalTransform transform)
    {
        float3 separation = float3.zero;
        float3 alignment = transform.Forward();
        float3 cohesion = transform.Position;

        foreach(LocalTransform boid in nearbyBoids)
        {
            if (math.distance(transform.Position, boid.Position) >= 10.0f)
                continue;

            separation += GetSeparationVector(transform, boid);
            alignment += boid.Forward();
            cohesion += boid.Position;
        }

        float average = 1.0f / nearbyBoids.Length;
        alignment *= average;
        cohesion *= average;
        cohesion = math.normalize(cohesion - transform.Position);

        data.direction = separation + alignment + cohesion;
        transform = transform.Rotate(quaternion.EulerXYZ(math.normalize(data.direction)));
        transform = transform.Translate(data.direction * 2.0f * DeltaTime);
    }
}
