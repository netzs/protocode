using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class DeadReturnGroupSystem : IEcsInitSystem, IEcsRunSystem
    {
        private Config _config;
        private EcsFilterExt<DestroyComponent, BaseUnitComponent, UnitGroupComponent> _filterUnit;
        private EcsPool<UnitGroupDataComponent> _groupPool;
        private EcsWorld _world;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _world = systems.GetWorld();
            _filterUnit.Validate(_world);
            _groupPool = _config.WorldGroup.GetPool<UnitGroupDataComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterUnit.Filter())
            {
                ref var gr = ref _filterUnit.Inc3().Get(entity);
                if (gr.Group.Unpack(_config.WorldGroup, out var g))
                {
                    var pack = _world.PackEntity(entity);
                    ref var group = ref _groupPool.Get(g);
                    for (var i = 0; i < group.Units.Length; i++)
                    {
                        if (group.Units[i].Equals(pack))
                        {
                            group.Units[i] = _config.EmptyEcsPackEntity;
                            group.Count--;
                            break;
                        }
                    }
                }

                _filterUnit.Inc3().Del(entity);
            }
        }
    }
}