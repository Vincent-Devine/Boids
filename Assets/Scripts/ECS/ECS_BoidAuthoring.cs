using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BoidAuthoring : MonoBehaviour
{
    class Baker : Baker<BoidAuthoring>
    {
        [System.Obsolete]
        public override void Bake(BoidAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BoidTag());

            var random = new Unity.Mathematics.Random((uint)entity.Index + 1);
            float2 dir = math.normalize(random.NextFloat2(-1, 1));

            AddComponent(entity, new BoidVelocity { Value = dir });
            AddComponent(entity, new BoidAnimOffset { Value = 0f });
        }
    }
}