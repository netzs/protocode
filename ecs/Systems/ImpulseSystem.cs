using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class ImpulseSystem : IEcsInitSystem, IEcsRunSystem
    {
        private Config _config;
        private EcsWorld _world;

        private EcsFilterExt<BaseUnitComponent, ImpulseComponent> _filter;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _config = systems.GetShared<Config>();
            _filter.Validate(_world);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter.Filter())
            {
                ref var unit = ref _filter.Inc1().Get(entity);
                ref var imp = ref _filter.Inc2().Get(entity);

                unit.cur.position = unit.Pos + imp.Pos * imp.Factor;
                imp.Pos *= 1 - imp.Factor;
                if (imp.Pos.sqrMagnitude < 0.1f)
                {
                    _filter.Inc2().Del(entity);
                }
            }
        }
    }
}