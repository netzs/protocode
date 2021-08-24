using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class ActionMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<WaitCommandComponent, BaseUnitComponent, UnitActionsComponent, MoveConfigComponent>
            _waitBaseFilter;

        private Config _config;
        private EcsWorld _world;
        private EcsPool<StartMoveCommandComponent> _startMovePool;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _world = systems.GetWorld();
            _waitBaseFilter.Validate(_world);
            _startMovePool = _world.GetPool<StartMoveCommandComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _waitBaseFilter.Filter())
            {
                ref var wait = ref _waitBaseFilter.Inc1().Get(entity);
                if (wait.Time < _config.Time)
                {
                    ref var unit = ref _waitBaseFilter.Inc2().Get(entity);

                    ref var act = ref _waitBaseFilter.Inc3().Get(entity);
                    var minDelay = act.GetMinDelay();

                    if (act.IsHasAction && minDelay < _config.Time)
                    {
                        var target = EcsUtility.FindSingleTarget(systems, unit, act.Actions, true);

                        if (target >= 0)
                        {
                            var ps = _waitBaseFilter.Inc2().Get(target).Pos;
                            if ((ps - unit.Pos).sqrMagnitude < Config.SqrDeltaLen)
                            {
                                // wait.Time = Math.Max(minDelay, _config.Time + Config.TimeReAction);
                            }
                            else
                            {
                                _waitBaseFilter.Inc1().Del(entity);
                                ref var snc = ref _startMovePool.Add(entity);
                                snc.Pos = ps;
                                snc.Time = Config.TimeMove + _config.Time;
                            }
                        }
                    }
                    else
                    {
                        wait.Time = minDelay;
                    }
                }
            }
        }
    }
}