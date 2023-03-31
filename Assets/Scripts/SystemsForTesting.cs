using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class SystemsForTesting : SystemBase
{
    private EntityQuery _query;
    
    protected override void OnCreate()
    {
        _query = GetEntityQuery(new EntityQueryDesc()
        {
            All = new[]
            {
                ComponentType.ReadWrite<LocalTransform>(),
                ComponentType.ReadWrite<SomeComp>()
            },
            None = new[]
            {
                ComponentType.ReadOnly<Destroyed>()
            }
        });
    }

    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        Dependency = new SomeJob()
        {
            DeltaTime        = deltaTime
        }.ScheduleParallel(_query, Dependency);
    }

    [BurstCompile]
    partial struct SomeJob : IJobEntity
    {
        public            float                           DeltaTime;
        
        void Execute(in  Entity e, ref LocalTransform transform, ref SomeComp someComp)
        {
            var val = transform.Position;
            transform.Position      = val;
            someComp.Value = DeltaTime;
        }
    }
}

[BurstCompile]
public partial struct SpawnSystem : ISystem
{
    public void OnUpdate(ref  SystemState state)
    {
        var elapsed = SystemAPI.Time.ElapsedTime;

        if (elapsed < 5) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var job = new SpawnTowerJob()
        {
            Ecb          = ecb,
            PrefabHolder = SystemAPI.GetSingleton<PrefabHolder>()
        };

        state.Dependency = job.Schedule(state.Dependency);
        state.Enabled = false;
    }
    
    [BurstCompile]
    private struct SpawnTowerJob : IJob
    {
        [ReadOnly]  public PrefabHolder        PrefabHolder;
        [WriteOnly] public EntityCommandBuffer Ecb;
        public             int                 c;
            
        public void Execute()
        {
            var obj = Ecb.Instantiate(PrefabHolder.Entity);
            Ecb.AddComponent(obj, new SomeComp());
            Ecb.SetComponent(obj, new LocalTransform()
            {
                Position = 1,
                Rotation = quaternion.identity,
                Scale = 1
            });
        }
    }
}
