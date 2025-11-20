
using Unity.Entities;
using UnityEngine;

public class BoidConfigAuthoring : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5f;

    [Header("Flocking")]
    public float NeighborDistance = 5f;
    public float SeparationDistance = 1f;
    public float SeparationWeight = 1.5f;
    public float AlignmentWeight = 1.0f;
    public float CohesionWeight = 1.0f;

    [Header("Obstacle Avoidance")]
    public float ObstacleViewDistance = 3f;
    public float ObstacleWeight = 20f;
    public LayerMask ObstacleLayerMask;

    [Header("Arena Bounds")]
    public Vector2 BoundsMin = new Vector2(-35, -35);
    public Vector2 BoundsMax = new Vector2(35, 35);

    class Baker : Baker<BoidConfigAuthoring>
    {
        public override void Bake(BoidConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new BoidConfig
            {
                MoveSpeed = authoring.MoveSpeed,
                NeighborDistance = authoring.NeighborDistance,
                SeparationDistance = authoring.SeparationDistance,
                SeparationWeight = authoring.SeparationWeight,
                AlignmentWeight = authoring.AlignmentWeight,
                CohesionWeight = authoring.CohesionWeight,
                ObstacleViewDistance = authoring.ObstacleViewDistance,
                ObstacleWeight = authoring.ObstacleWeight,
                ObstacleLayerMask = (uint)authoring.ObstacleLayerMask.value,
                BoundsMin = authoring.BoundsMin,
                BoundsMax = authoring.BoundsMax
            });
        }
    }
}