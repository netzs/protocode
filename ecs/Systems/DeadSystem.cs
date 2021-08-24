using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class DeadSystem : IEcsInitSystem, IEcsRunSystem
    {
        private Config _config;
        private EcsWorld _world;
        private EcsFilterExt<DeadComponent, BaseUnitComponent, UnitComponent> _filterUnit;
        private EcsFilterExt<DeadComponent, BaseUnitComponent, StoneComponent> _filterStone;
        private EcsPool<HpComponent> _hpPool;
        private EcsPool<DestroyComponent> _destroyPool;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _config = systems.GetShared<Config>();
            _filterStone.Validate(_world);
            _filterUnit.Validate(_world);
            _hpPool = _world.GetPool<HpComponent>();
            _destroyPool = _world.GetPool<DestroyComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterUnit.Filter())
            {
                ref var hp = ref _hpPool.Get(entity);
                hp.isNeedUpdate = true;

                if (_filterUnit.Inc1().Get(entity).Time + 2 < _config.Time)
                {
                    var pos = _filterUnit.Inc2().Get(entity).Pos;
                    pos.y -= _config.DeltaTime * 0.2f;
                    _filterUnit.Inc2().Get(entity).cur.position = pos;
                }

                if (_filterUnit.Inc1().Get(entity).Time + 5 < _config.Time && !_destroyPool.Has(entity))
                {
                    _destroyPool.Add(entity);
                    // Object.Destroy(_filterUnit.Inc2().Get(entity).cur.gameObject);
                    // _world.DelEntity(entity);
                }
            }

            foreach (var entity in _filterStone.Filter())
            {
                    _destroyPool.Add(entity);
            }
        }
    }
}