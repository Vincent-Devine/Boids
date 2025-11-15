using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class BirdAuthoring : MonoBehaviour
{
    public float speed;

    private class Baker : Baker<BirdAuthoring>
    {
        public override void Bake(BirdAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(1234);
            AddComponent(entity, new BoidData
            {
                direction = random.NextFloat3Direction(),
                speed = authoring.speed
            });
        }
    }
}
