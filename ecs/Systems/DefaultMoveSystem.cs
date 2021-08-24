using ecs.Components;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedFilters;
using UnityEngine.AI;

namespace ecs.Systems
{
    internal class DefaultMoveSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilterExt<WaitCommandComponent, BaseUnitComponent, MoveConfigComponent, DefaultMove> _waitBaseFilter;
        private Config _config;
        private EcsWorld _world;

        private EcsPool<MoveCommandComponent> _movePool;
        private EcsPool<AnimatorComponent> _animatorPool;

        public void Init(EcsSystems systems)
        {
            _config = systems.GetShared<Config>();
            _world = systems.GetWorld();
            _waitBaseFilter.Validate(_world);
            _movePool = _world.GetPool<MoveCommandComponent>();
            _animatorPool = _world.GetPool<AnimatorComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _waitBaseFilter.Filter())
            {
                ref var wait = ref _waitBaseFilter.Inc1().Get(entity);
                if (wait.Time < _config.Time && wait.IsReWait)
                {
                    ref var unit = ref _waitBaseFilter.Inc2().Get(entity);

                    var ps = _waitBaseFilter.Inc4().Get(entity).Pos;

                    if ((ps - unit.Pos).sqrMagnitude < 0.1f * Config.SqrDeltaLen)
                    {
                    }
                    else
                    {
                        _waitBaseFilter.Inc1().Del(entity);
                        ref var mv = ref _movePool.Add(entity);
                        mv.Pos = ps;
                        mv.Time = Config.TimeMove + _config.Time;

                        var agent = _waitBaseFilter.Inc3().Get(entity).agent;
                        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

                        agent.SetDestination(ps);

                        // var path = new NavMeshPath();
                        // agent.CalculatePath(ps, path);
                        // if (path.status == NavMeshPathStatus.PathComplete)
                        // {
                        //     agent.SetPath(path);
                        // }

                        if (_animatorPool.Has(entity))
                        {
                            ref var anim = ref _animatorPool.Get(entity);
                            anim.isNeedUpdate = true;
                            anim.runTrigger = true;
                        }
                    }
                }
            }
        }
    }
}