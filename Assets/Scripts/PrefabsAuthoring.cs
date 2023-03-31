using Unity.Entities;
using UnityEngine;

public struct Destroyed : IComponentData { }

public struct SomeComp : IComponentData
{
    public float Value;
}

public struct PrefabHolder : IComponentData
{
    public Entity Entity;
}

public class PrefabsAuthoring : MonoBehaviour
{
    public GameObject prefab;
}

public class PrefabsBaker : Baker<PrefabsAuthoring> 
{
    public override void Bake(PrefabsAuthoring authoring)
    {
        var e = GetEntity(TransformUsageFlags.None);
        AddComponent(e, new PrefabHolder()
        {
            Entity = GetEntity(authoring.prefab, TransformUsageFlags.Renderable | TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace)
        });
    }
}
