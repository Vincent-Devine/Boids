using System;
using Unity.Entities;
using UnityEngine;
public class ECS_BoidAuthoring : MonoBehaviour
{
    public float CellRadius = 8.0f;
    public float SeparationWeight = 1.0f;
    public float AlignmentWeight = 1.0f;
    public float TargetWeight = 2.0f;
    public float ObstacleAversionDistance = 30.0f;
    public float MoveSpeed = 25.0f;

    class Baker : Baker<ECS_BoidAuthoring>
    {
        public override void Bake(ECS_BoidAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable | TransformUsageFlags.WorldSpace);
            AddSharedComponent(entity, new ECS_Boid
            {
                CellRadius = authoring.CellRadius,
                SeparationWeight = authoring.SeparationWeight,
                AlignmentWeight = authoring.AlignmentWeight,
                TargetWeight = authoring.TargetWeight,
                ObstacleAversionDistance = authoring.ObstacleAversionDistance,
                MoveSpeed = authoring.MoveSpeed
            });
        }
    }
}

[Serializable]
public struct ECS_Boid : ISharedComponentData
{
    public float CellRadius;
    public float SeparationWeight;
    public float AlignmentWeight;
    public float TargetWeight;
    public float ObstacleAversionDistance;
    public float MoveSpeed;
}