using System;
using Unity.Entities;
using UnityEngine;

public class ECS_ObstacleAuthoringBaker : Baker<ECS_ObstacleAuthoring>
{
    public override void Bake(ECS_ObstacleAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Renderable);
        AddComponent(entity, new ECS_Obstacle());
    }
}

public struct ECS_Obstacle : IComponentData
{
}

public class ECS_ObstacleAuthoring : MonoBehaviour
{
}
