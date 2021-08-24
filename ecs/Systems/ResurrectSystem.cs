using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class ResurrectSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<DeadComponent, BaseUnitComponent, HpComponent, ResurrectComponent> _filter;
        private Config _config;
        private EcsPool<WaitCommandComponent> _waitPool;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _filter.Validate(_config.WorldDefault);
            _waitPool = _config.WorldDefault.GetPool<WaitCommandComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var e in _filter.Filter())
            {
                _filter.Inc1().Del(e);
                _filter.Inc4().Del(e);
                ref var hp = ref _filter.Inc3().Get(e);
                hp.value = hp.maxValue;
                hp.isNeedUpdate = true;

                _waitPool.Add(e);

                if (_config.WorldDefault.GetPool<MoveConfigComponent>().Has(e))
                {
                    _config.WorldDefault.GetPool<MoveConfigComponent>().Get(e).agent.enabled = true;
                }
            }
        }
    }
}