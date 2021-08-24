using System;
using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class DeadDropSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<DeadComponent, BaseUnitComponent, HpComponent> _filter;
        private Config _config;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldDefault);
        }

        public void Run(EcsSystems systems)
        {
            foreach (var e in _filter.Filter())
            {
                ref var dead = ref _filter.Inc1().Get(e);

                if (!dead.IsGetDrop)
                {
                    dead.IsGetDrop = true;
                    ref var unit = ref _filter.Inc2().Get(e);
                    if (Config.TeamId != unit.teamId)
                    {
                        _config.Coin += 1 + _filter.Inc3().Get(e).maxValue / 10;
                    }
                }
            }
        }
    }
}