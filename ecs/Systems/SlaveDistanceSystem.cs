using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine;

namespace ecs.Systems
{
    internal class SlaveDistanceSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<WaitCommandComponent, BaseUnitComponent, UnitGroupComponent> _filter;
        private Config _config;
        private EcsWorld _world;
        private EcsPool<UnitGroupDataComponent> _groupPool;
        private EcsPool<StartMoveCommandComponent> _startMovePool;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _world = systems.GetWorld();
            _filter.Validate(_world);
            _config = systems.GetShared<Config>();
            _groupPool = _config.WorldGroup.GetPool<UnitGroupDataComponent>();
            _startMovePool = _world.GetPool<StartMoveCommandComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                ref var unit = ref _filter.Inc2().Get(entity);
                ref var unitGroupComponent = ref _filter.Inc3().Get(entity);
                if (unitGroupComponent.Group.Unpack(_config.WorldGroup, out var g))
                {
                    ref var gr = ref _groupPool.Get(g);
                    if (gr.Leader.Unpack(_world, out var le))
                    {
                        ref var leader = ref _filter.Inc2().Get(le);
                        if ((leader.Pos - unit.Pos).sqrMagnitude > gr.Distance * gr.Distance)
                        {
                            _filter.Inc1().Del(entity);
                            ref var snc = ref _startMovePool.Add(entity);
                            snc.Pos = leader.Pos;
                            snc.Time = Config.TimeMove + _config.Time;
                        }
                    }
                    else
                    {
                        Debug.Log("not found");
                    }
                }
                else
                {
                    Debug.Log("not found");
                }
            }
        }
    }
}