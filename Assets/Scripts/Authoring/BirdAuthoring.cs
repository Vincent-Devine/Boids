using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class BirdAuthoring : MonoBehaviour
{
    public float speed;
    public float3 direction;

    private class Baker : Baker<BirdAuthoring>
    {
        public override void Bake(BirdAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DirectionData { direction = authoring.direction });
            AddComponent(entity, new SpeedData { speed = authoring.speed });
        }
    }
}
