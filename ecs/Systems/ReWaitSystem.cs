using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;

namespace ecs.Systems
{
    internal class ReWaitSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilterExt<WaitCommandComponent> _waitFilter;
        private Config _config;
        private EcsPool<AnimatorComponent> _animatorPool;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _waitFilter.Validate(_world);
            _config = systems.GetShared<Config>();
            _animatorPool = _world.GetPool<AnimatorComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _waitFilter.Filter())
            {
                ref var wait = ref _waitFilter.Inc1().Get(entity);
                if (wait.IsReWait && wait.Time < _config.Time)
                {
                    wait.Time = _config.Time + _config.GameConfig.waitTime;

                    if (_animatorPool.Has(entity))
                    {
                        ref var anim = ref _animatorPool.Get(entity);
                        anim.isNeedUpdate = true;
                        anim.idleTrigger = true;
                    }
                }

                wait.IsReWait = true;
            }
        }
    }
}